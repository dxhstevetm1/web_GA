using FacebookCommentAnalyzer.API.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;
using System.Globalization;

namespace FacebookCommentAnalyzer.API.Services
{
    public class WebScrapingService : IWebScrapingService, IDisposable
    {
        private readonly ILogger<WebScrapingService> _logger;
        private readonly IUrlParserService _urlParser;
        private IWebDriver? _driver;
        private bool _disposed = false;

        public WebScrapingService(ILogger<WebScrapingService> logger, IUrlParserService urlParser)
        {
            _logger = logger;
            _urlParser = urlParser;
        }

        public async Task<ScrapedPostData> ScrapePostCommentsAsync(string postUrl, ScrapingOptions? options = null)
        {
            options ??= new ScrapingOptions();
            
            try
            {
                InitializeDriver(options.UseHeadlessBrowser);
                
                var postData = new ScrapedPostData
                {
                    SourceUrl = postUrl,
                    ScrapedAt = DateTime.Now
                };

                // Navigate to post
                _driver!.Navigate().GoToUrl(postUrl);
                await WaitForPageLoad();

                // Get post information
                postData.PostInfo = await GetPostInfoAsync(postUrl);

                // Set comments sorting to oldest first if requested
                if (options.SortOrder == SortOrder.OldestFirst)
                {
                    await SetCommentsSortOrder("oldest");
                }

                // Scrape comments
                postData.Comments = await ScrapeCommentsFromPage(options);
                postData.TotalCommentsFound = postData.Comments.Count;

                // Check who shared the post
                await CheckCommenterShareStatus(postData.Comments, postUrl);

                return postData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scraping post comments from URL: {Url}", postUrl);
                throw;
            }
        }

        public async Task<List<ScrapedComment>> ScrapeCommentsAsync(string postUrl, ScrapingOptions? options = null)
        {
            var postData = await ScrapePostCommentsAsync(postUrl, options);
            return postData.Comments;
        }

        public async Task<ScrapedPostInfo> GetPostInfoAsync(string postUrl)
        {
            try
            {
                var postInfo = new ScrapedPostInfo
                {
                    PostUrl = postUrl
                };

                if (_driver == null)
                {
                    InitializeDriver(true);
                    _driver!.Navigate().GoToUrl(postUrl);
                    await WaitForPageLoad();
                }

                // Extract post content
                try
                {
                    var contentElement = _driver.FindElement(By.CssSelector("[data-ad-preview='message']"));
                    postInfo.Content = contentElement?.Text ?? "";
                }
                catch { }

                // Extract author information
                try
                {
                    var authorElement = _driver.FindElement(By.CssSelector("h3 a, [role='link'] strong"));
                    postInfo.AuthorName = authorElement?.Text ?? "";
                    postInfo.AuthorProfileUrl = authorElement?.GetAttribute("href") ?? "";
                }
                catch { }

                // Extract post time
                try
                {
                    var timeElement = _driver.FindElement(By.CssSelector("[data-testid='story-subtitle'] a"));
                    var timeText = timeElement?.GetAttribute("aria-label") ?? timeElement?.Text ?? "";
                    postInfo.PostTime = ParseFacebookTime(timeText);
                }
                catch { }

                // Extract engagement metrics
                try
                {
                    var likesElement = _driver.FindElement(By.CssSelector("[aria-label*='reactions']"));
                    postInfo.LikesCount = ExtractNumberFromText(likesElement?.GetAttribute("aria-label") ?? "");
                }
                catch { }

                try
                {
                    var commentsElement = _driver.FindElement(By.CssSelector("[aria-label*='comments']"));
                    postInfo.CommentsCount = ExtractNumberFromText(commentsElement?.GetAttribute("aria-label") ?? "");
                }
                catch { }

                // Check if it's a group post
                var urlInfo = _urlParser.ParseFacebookUrl(postUrl);
                postInfo.IsGroupPost = urlInfo.IsGroupPost;
                postInfo.GroupId = urlInfo.GroupId;

                if (postInfo.IsGroupPost)
                {
                    try
                    {
                        var groupElement = _driver.FindElement(By.CssSelector("a[href*='/groups/']"));
                        postInfo.GroupName = groupElement?.Text ?? "";
                    }
                    catch { }
                }

                return postInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post info from URL: {Url}", postUrl);
                return new ScrapedPostInfo { PostUrl = postUrl };
            }
        }

        public async Task<bool> CheckUserSharedPostAsync(string postUrl, string userProfileUrl)
        {
            try
            {
                if (_driver == null)
                    InitializeDriver(true);

                // Navigate to user's profile
                _driver!.Navigate().GoToUrl(userProfileUrl);
                await WaitForPageLoad();

                // Search for shared posts in user's timeline
                // This is a simplified implementation - in reality, you'd need to scroll through posts
                var posts = _driver.FindElements(By.CssSelector("[role='article']"));
                
                foreach (var post in posts.Take(20)) // Check recent posts
                {
                    try
                    {
                        var postText = post.Text;
                        var links = post.FindElements(By.TagName("a"));
                        
                        foreach (var link in links)
                        {
                            var href = link.GetAttribute("href");
                            if (!string.IsNullOrEmpty(href) && href.Contains(ExtractPostIdentifier(postUrl)))
                            {
                                return true;
                            }
                        }
                    }
                    catch { continue; }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user share status");
                return false;
            }
        }

        private void InitializeDriver(bool headless = true)
        {
            if (_driver != null)
                return;

            var options = new ChromeOptions();
            
            if (headless)
                options.AddArgument("--headless");
            
            options.AddArguments(
                "--no-sandbox",
                "--disable-dev-shm-usage",
                "--disable-gpu",
                "--disable-blink-features=AutomationControlled",
                "--disable-extensions",
                "--disable-plugins",
                "--disable-images", // Speed up loading
                "--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
            );

            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        private async Task WaitForPageLoad()
        {
            var wait = new WebDriverWait(_driver!, TimeSpan.FromSeconds(10));
            await Task.Run(() =>
            {
                wait.Until(driver => ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState").Equals("complete"));
            });
            
            // Additional wait for Facebook's dynamic content
            await Task.Delay(3000);
        }

        private async Task SetCommentsSortOrder(string order = "oldest")
        {
            try
            {
                // Look for sort dropdown or button
                var sortElements = _driver!.FindElements(By.CssSelector("[aria-label*='Sort'], [aria-label*='sort']"));
                
                foreach (var element in sortElements)
                {
                    if (element.Text.ToLower().Contains("sort") || element.GetAttribute("aria-label").ToLower().Contains("sort"))
                    {
                        element.Click();
                        await Task.Delay(1000);
                        
                        // Look for "Oldest" option
                        var oldestOption = _driver.FindElements(By.XPath("//*[contains(text(), 'Oldest') or contains(text(), 'oldest')]"));
                        if (oldestOption.Any())
                        {
                            oldestOption.First().Click();
                            await Task.Delay(2000);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not set comments sort order");
            }
        }

        private async Task<List<ScrapedComment>> ScrapeCommentsFromPage(ScrapingOptions options)
        {
            var comments = new List<ScrapedComment>();
            var scrollAttempts = 0;
            var lastCommentCount = 0;

            try
            {
                // Load more comments by scrolling
                while (scrollAttempts < options.MaxScrollAttempts && comments.Count < options.MaxCommentsToScrape)
                {
                    // Try to click "View more comments" buttons
                    var viewMoreButtons = _driver!.FindElements(By.XPath("//*[contains(text(), 'View more comments') or contains(text(), 'more comments')]"));
                    foreach (var button in viewMoreButtons)
                    {
                        try
                        {
                            if (button.Displayed && button.Enabled)
                            {
                                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", button);
                                await Task.Delay(options.ScrollDelayMs);
                            }
                        }
                        catch { }
                    }

                    // Scroll down to load more
                    ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                    await Task.Delay(options.ScrollDelayMs);

                    // Extract comments from current page state
                    var currentComments = ExtractCommentsFromDOM();
                    
                    if (currentComments.Count == lastCommentCount)
                    {
                        scrollAttempts++;
                    }
                    else
                    {
                        lastCommentCount = currentComments.Count;
                        scrollAttempts = 0; // Reset if we're still finding new comments
                    }

                    // Merge new comments (avoid duplicates)
                    foreach (var comment in currentComments)
                    {
                        if (!comments.Any(c => c.CommentId == comment.CommentId || 
                                              (c.AuthorName == comment.AuthorName && c.Content == comment.Content && 
                                               Math.Abs((c.CommentTime ?? DateTime.MinValue).Subtract(comment.CommentTime ?? DateTime.MinValue).TotalMinutes) < 1)))
                        {
                            comments.Add(comment);
                        }
                    }

                    if (comments.Count >= options.MaxCommentsToScrape)
                        break;
                }

                // Sort comments by time (oldest first by default)
                if (options.SortOrder == SortOrder.OldestFirst)
                {
                    comments = comments.OrderBy(c => c.CommentTime ?? DateTime.MinValue).ToList();
                }
                else if (options.SortOrder == SortOrder.NewestFirst)
                {
                    comments = comments.OrderByDescending(c => c.CommentTime ?? DateTime.MinValue).ToList();
                }

                return comments.Take(options.MaxCommentsToScrape).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scraping comments from page");
                return comments;
            }
        }

        private List<ScrapedComment> ExtractCommentsFromDOM()
        {
            var comments = new List<ScrapedComment>();

            try
            {
                // Different selectors for comment containers in Facebook
                var commentSelectors = new[]
                {
                    "[aria-label*='Comment by']",
                    "[role='article'][aria-label*='Comment']",
                    "div[data-testid='UFI2Comment/root']",
                    ".x1y1aw1k", // Facebook's comment container class (may change)
                    "div[aria-label][role='article']"
                };

                var commentElements = new List<IWebElement>();
                
                foreach (var selector in commentSelectors)
                {
                    try
                    {
                        var elements = _driver!.FindElements(By.CssSelector(selector));
                        commentElements.AddRange(elements);
                        if (elements.Count > 0) break; // Use the first working selector
                    }
                    catch { continue; }
                }

                foreach (var element in commentElements)
                {
                    try
                    {
                        var comment = ExtractCommentData(element);
                        if (!string.IsNullOrEmpty(comment.Content) || !string.IsNullOrEmpty(comment.AuthorName))
                        {
                            comments.Add(comment);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Error extracting individual comment");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting comments from DOM");
            }

            return comments;
        }

        private ScrapedComment ExtractCommentData(IWebElement element)
        {
            var comment = new ScrapedComment();

            try
            {
                // Extract author name and profile URL
                var authorLink = element.FindElements(By.CssSelector("a[role='link']")).FirstOrDefault();
                if (authorLink != null)
                {
                    comment.AuthorName = authorLink.Text;
                    comment.AuthorProfileUrl = authorLink.GetAttribute("href") ?? "";
                }

                // Extract comment content
                var contentElement = element.FindElements(By.CssSelector("div[dir='auto']")).FirstOrDefault(e => !string.IsNullOrEmpty(e.Text));
                if (contentElement != null)
                {
                    comment.Content = contentElement.Text;
                }

                // Extract comment time
                var timeElement = element.FindElements(By.CssSelector("a[role='link'][aria-label*='ago'], a[aria-label*='at']")).FirstOrDefault();
                if (timeElement != null)
                {
                    var timeText = timeElement.GetAttribute("aria-label") ?? timeElement.Text;
                    comment.CommentTime = ParseFacebookTime(timeText);
                }

                // Extract likes count
                var likeElement = element.FindElements(By.CssSelector("[aria-label*='reactions'], [aria-label*='likes']")).FirstOrDefault();
                if (likeElement != null)
                {
                    comment.LikesCount = ExtractNumberFromText(likeElement.GetAttribute("aria-label") ?? "");
                }

                // Generate a pseudo-unique ID
                comment.CommentId = GenerateCommentId(comment);

                return comment;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error extracting comment data from element");
                return comment;
            }
        }

        private async Task CheckCommenterShareStatus(List<ScrapedComment> comments, string originalPostUrl)
        {
            foreach (var comment in comments)
            {
                try
                {
                    if (!string.IsNullOrEmpty(comment.AuthorProfileUrl))
                    {
                        comment.HasSharedPost = await CheckUserSharedPostAsync(originalPostUrl, comment.AuthorProfileUrl);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error checking share status for commenter: {AuthorName}", comment.AuthorName);
                }
            }
        }

        private DateTime? ParseFacebookTime(string timeText)
        {
            try
            {
                if (string.IsNullOrEmpty(timeText))
                    return null;

                timeText = timeText.ToLower();

                // Handle relative times like "5m", "2h", "3d"
                var relativeMatch = Regex.Match(timeText, @"(\d+)\s*(m|h|d|w)");
                if (relativeMatch.Success)
                {
                    var value = int.Parse(relativeMatch.Groups[1].Value);
                    var unit = relativeMatch.Groups[2].Value;

                    return unit switch
                    {
                        "m" => DateTime.Now.AddMinutes(-value),
                        "h" => DateTime.Now.AddHours(-value),
                        "d" => DateTime.Now.AddDays(-value),
                        "w" => DateTime.Now.AddDays(-value * 7),
                        _ => null
                    };
                }

                // Handle "yesterday"
                if (timeText.Contains("yesterday"))
                    return DateTime.Today.AddDays(-1);

                // Try to parse absolute dates
                var dateMatch = Regex.Match(timeText, @"(\w+\s+\d+(?:,\s+\d+)?)", RegexOptions.IgnoreCase);
                if (dateMatch.Success)
                {
                    if (DateTime.TryParse(dateMatch.Value, out var parsedDate))
                        return parsedDate;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private int ExtractNumberFromText(string text)
        {
            var match = Regex.Match(text, @"(\d+(?:,\d+)*(?:\.\d+)?)\s*[KkMm]?");
            if (match.Success)
            {
                var numberStr = match.Groups[1].Value.Replace(",", "");
                if (double.TryParse(numberStr, out var number))
                {
                    // Handle K and M suffixes
                    if (text.ToLower().Contains("k"))
                        number *= 1000;
                    else if (text.ToLower().Contains("m"))
                        number *= 1000000;
                    
                    return (int)number;
                }
            }
            return 0;
        }

        private string GenerateCommentId(ScrapedComment comment)
        {
            // Generate a pseudo-unique ID based on author and content
            var content = $"{comment.AuthorName}_{comment.Content}_{comment.CommentTime?.Ticks}";
            return Math.Abs(content.GetHashCode()).ToString();
        }

        private string ExtractPostIdentifier(string postUrl)
        {
            var urlInfo = _urlParser.ParseFacebookUrl(postUrl);
            return urlInfo.PostId;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        _driver?.Quit();
                        _driver?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error disposing WebDriver");
                    }
                }
                _disposed = true;
            }
        }
    }
}