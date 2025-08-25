using FacebookCommentAnalyzer.API.Models;
using Microsoft.Playwright;

namespace FacebookCommentAnalyzer.API.Services
{
	public class FacebookScrapeService : IFacebookScrapeService
	{
		private readonly ILogger<FacebookScrapeService> _logger;

		public FacebookScrapeService(ILogger<FacebookScrapeService> logger)
		{
			_logger = logger;
		}

		public async Task<List<FacebookComment>> ScrapeCommentsAsync(string postUrl, bool checkShare, CancellationToken cancellationToken = default)
		{
			var normalizedUrl = NormalizeToMobileBasic(postUrl);
			var comments = new List<FacebookComment>();

			try
			{
				using var playwright = await Playwright.CreateAsync();
				await EnsureBrowsersInstalled(playwright, cancellationToken);

				await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
				{
					headless = true
				});
				var context = await browser.NewContextAsync(new BrowserNewContextOptions
				{
					UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123 Safari/537.36",
					Locale = "en-US"
				});
				var page = await context.NewPageAsync();

				await page.GotoAsync(normalizedUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });

				// If there is a link to view full story, click it
				var fullStory = await page.QuerySelectorAsync("a:has-text('Full Story')");
				if (fullStory != null)
				{
					await Task.WhenAll(page.WaitForLoadStateAsync(LoadState.NetworkIdle), fullStory.ClickAsync());
				}

				// Load more comments by following pagination on mbasic
				while (true)
				{
					comments.AddRange(await ExtractCommentsFromCurrentPageAsync(page));

					var moreLink = await page.QuerySelectorAsync("a:has-text('View more comments')");
					if (moreLink == null)
					{
						break;
					}
					await Task.WhenAll(page.WaitForLoadStateAsync(LoadState.NetworkIdle), moreLink.ClickAsync());
				}

				// De-duplicate by comment Id if present
				comments = comments
					.GroupBy(c => string.IsNullOrWhiteSpace(c.Id) ? Guid.NewGuid().ToString() : c.Id)
					.Select(g => g.First())
					.OrderBy(c => c.CreatedTime)
					.ToList();

				if (checkShare)
				{
					await PopulateShareFlagsAsync(page, comments, postUrl, cancellationToken);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error scraping Facebook comments");
			}

			return comments;
		}

		private static string NormalizeToMobileBasic(string url)
		{
			try
			{
				var uri = new Uri(url);
				var builder = new UriBuilder(uri)
				{
					Host = uri.Host.Replace("www.facebook.com", "mbasic.facebook.com").Replace("m.facebook.com", "mbasic.facebook.com")
				};
				return builder.Uri.ToString();
			}
			catch
			{
				return url;
			}
		}

		private static async Task EnsureBrowsersInstalled(IPlaywright playwright, CancellationToken cancellationToken)
		{
			try
			{
				// No-op in most environments if already installed. Programmatic install fallback.
				await Microsoft.Playwright.Program.Main(new[] { "install" });
			}
			catch
			{
				// Ignore install errors; CreateAsync will fail later if missing
			}
		}

		private static async Task<List<FacebookComment>> ExtractCommentsFromCurrentPageAsync(IPage page)
		{
			var results = new List<FacebookComment>();

			// mbasic renders comments inside divs following the story, using anchors for user and small/abbr for time
			var commentBlocks = await page.QuerySelectorAllAsync("div[role='article'] div > div");
			foreach (var block in commentBlocks)
			{
				try
				{
					var userAnchor = await block.QuerySelectorAsync("a[href*='facebook.com']");
					var messageSpan = await block.QuerySelectorAsync("div, span, p");
					var timeAbbr = await block.QuerySelectorAsync("abbr, small");

					if (userAnchor == null || messageSpan == null)
					{
						continue;
					}

					var userName = (await userAnchor.InnerTextAsync()).Trim();
					var userHref = await userAnchor.GetAttributeAsync("href");
					var message = (await messageSpan.InnerTextAsync()).Trim();
					var timeText = timeAbbr != null ? (await timeAbbr.InnerTextAsync()).Trim() : string.Empty;

					var created = ParseRelativeTime(timeText) ?? DateTime.UtcNow;

					results.Add(new FacebookComment
					{
						Id = string.Empty,
						Message = message,
						From = new FacebookUser
						{
							Id = string.Empty,
							Name = userName,
							Picture = new FacebookPicture { Data = new FacebookPictureData { Url = string.Empty } },
							ProfileUrl = ResolveAbsoluteUrl(page.Url, userHref ?? string.Empty)
						},
						CreatedTime = created,
						CommentCount = 0,
						LikeCount = 0,
						IsHidden = false,
						CanReply = false
					});
				}
				catch
				{
					// Ignore individual comment parse failures
				}
			}

			return results;
		}

		private static string ResolveAbsoluteUrl(string baseUrl, string href)
		{
			try
			{
				var baseUri = new Uri(baseUrl);
				return new Uri(baseUri, href).ToString();
			}
			catch
			{
				return href;
			}
		}

		private static DateTime? ParseRelativeTime(string text)
		{
			if (string.IsNullOrWhiteSpace(text)) return null;
			text = text.Trim().ToLowerInvariant();

			try
			{
				if (text.Contains("just now")) return DateTime.UtcNow;
				if (text.Contains("minute")) return DateTime.UtcNow.AddMinutes(-ExtractLeadingInt(text));
				if (text.Contains("hour")) return DateTime.UtcNow.AddHours(-ExtractLeadingInt(text));
				if (text.Contains("day")) return DateTime.UtcNow.AddDays(-ExtractLeadingInt(text));
				if (text.Contains("week")) return DateTime.UtcNow.AddDays(-7 * ExtractLeadingInt(text));
				if (text.Contains("month")) return DateTime.UtcNow.AddMonths(-Math.Max(1, ExtractLeadingInt(text)));
				if (text.Contains("year")) return DateTime.UtcNow.AddYears(-Math.Max(1, ExtractLeadingInt(text)));
			}
			catch { }

			return null;
		}

		private static int ExtractLeadingInt(string text)
		{
			var digits = new string(text.TakeWhile(c => char.IsDigit(c)).ToArray());
			return int.TryParse(digits, out var n) ? n : 1;
		}

		private static async Task PopulateShareFlagsAsync(IPage page, List<FacebookComment> comments, string targetPostUrl, CancellationToken cancellationToken)
		{
			// Limit concurrency to avoid rate-limits/blocks
			var semaphore = new SemaphoreSlim(2);
			var tasks = comments.Select(async comment =>
			{
				await semaphore.WaitAsync(cancellationToken);
				try
				{
					if (string.IsNullOrWhiteSpace(comment.From.ProfileUrl)) return;
					var profilePage = await page.Context.NewPageAsync();
					await profilePage.GotoAsync(NormalizeToMobileBasic(comment.From.ProfileUrl), new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 45000 });
					var found = await profilePage.Locator($"a[href*='{new Uri(targetPostUrl).AbsolutePath}']").First.Or(new LocatorFilterOptions()).CountAsync() > 0;
					comment.HasSharedPost = found;
					await profilePage.CloseAsync();
				}
				catch
				{
					comment.HasSharedPost = false;
				}
				finally
				{
					semaphore.Release();
				}
			});

			await Task.WhenAll(tasks);
		}
	}
}
