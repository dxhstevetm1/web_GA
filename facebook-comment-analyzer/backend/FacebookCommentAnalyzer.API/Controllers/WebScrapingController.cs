using FacebookCommentAnalyzer.API.Models;
using FacebookCommentAnalyzer.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FacebookCommentAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebScrapingController : ControllerBase
    {
        private readonly IWebScrapingService _webScrapingService;
        private readonly IUrlParserService _urlParserService;
        private readonly ILogger<WebScrapingController> _logger;

        public WebScrapingController(
            IWebScrapingService webScrapingService,
            IUrlParserService urlParserService,
            ILogger<WebScrapingController> logger)
        {
            _webScrapingService = webScrapingService;
            _urlParserService = urlParserService;
            _logger = logger;
        }

        /// <summary>
        /// Scrape toàn bộ dữ liệu comments từ Facebook post URL
        /// </summary>
        [HttpPost("scrape-post-comments")]
        public async Task<ActionResult<ScrapedPostData>> ScrapePostComments([FromBody] ScrapePostRequest request)
        {
            try
            {
                // Validate URL
                if (!_urlParserService.IsValidFacebookPostUrl(request.PostUrl))
                {
                    return BadRequest(new { Error = "Invalid Facebook post URL" });
                }

                var options = new ScrapingOptions
                {
                    MaxCommentsToScrape = request.MaxComments ?? 1000,
                    SortOrder = request.SortOrder ?? SortOrder.OldestFirst,
                    LoadReplies = request.LoadReplies ?? false,
                    LoadReactions = request.LoadReactions ?? false,
                    UseHeadlessBrowser = request.UseHeadlessBrowser ?? true,
                    ScrollDelayMs = request.ScrollDelayMs ?? 2000,
                    MaxScrollAttempts = request.MaxScrollAttempts ?? 50
                };

                var result = await _webScrapingService.ScrapePostCommentsAsync(request.PostUrl, options);
                
                return Ok(new
                {
                    Success = true,
                    Data = result,
                    Message = $"Successfully scraped {result.Comments.Count} comments from the post",
                    ScrapedAt = result.ScrapedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scraping post comments from URL: {Url}", request.PostUrl);
                return StatusCode(500, new 
                { 
                    Success = false,
                    Error = "Internal server error occurred while scraping comments",
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Lấy thông tin cơ bản của post từ URL
        /// </summary>
        [HttpPost("get-post-info")]
        public async Task<ActionResult<ScrapedPostInfo>> GetPostInfo([FromBody] PostInfoRequest request)
        {
            try
            {
                if (!_urlParserService.IsValidFacebookPostUrl(request.PostUrl))
                {
                    return BadRequest(new { Error = "Invalid Facebook post URL" });
                }

                var postInfo = await _webScrapingService.GetPostInfoAsync(request.PostUrl);
                
                return Ok(new
                {
                    Success = true,
                    Data = postInfo,
                    Message = "Successfully retrieved post information"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post info from URL: {Url}", request.PostUrl);
                return StatusCode(500, new 
                { 
                    Success = false,
                    Error = "Internal server error occurred while getting post info",
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Kiểm tra URL Facebook có hợp lệ không
        /// </summary>
        [HttpPost("validate-url")]
        public ActionResult<FacebookUrlInfo> ValidateUrl([FromBody] ValidateUrlRequest request)
        {
            try
            {
                var urlInfo = _urlParserService.ParseFacebookUrl(request.Url);
                var isValid = _urlParserService.IsValidFacebookPostUrl(request.Url);

                return Ok(new
                {
                    Success = true,
                    IsValid = isValid,
                    UrlInfo = urlInfo,
                    Message = isValid ? "Valid Facebook post URL" : "Invalid Facebook post URL"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating URL: {Url}", request.Url);
                return StatusCode(500, new 
                { 
                    Success = false,
                    Error = "Internal server error occurred while validating URL",
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Phân tích comments với filter nâng cao
        /// </summary>
        [HttpPost("analyze-comments")]
        public async Task<ActionResult<AnalyzedCommentsResult>> AnalyzeComments([FromBody] AnalyzeCommentsRequest request)
        {
            try
            {
                if (!_urlParserService.IsValidFacebookPostUrl(request.PostUrl))
                {
                    return BadRequest(new { Error = "Invalid Facebook post URL" });
                }

                var options = new ScrapingOptions
                {
                    MaxCommentsToScrape = request.MaxComments ?? 1000,
                    SortOrder = request.SortOrder ?? SortOrder.OldestFirst,
                    LoadReplies = request.LoadReplies ?? false,
                    LoadReactions = request.LoadReactions ?? false,
                    UseHeadlessBrowser = true
                };

                var scrapedData = await _webScrapingService.ScrapePostCommentsAsync(request.PostUrl, options);

                // Apply filters
                var filteredComments = FilterComments(scrapedData.Comments, request.Filters);

                // Create analysis result
                var result = new AnalyzedCommentsResult
                {
                    PostInfo = scrapedData.PostInfo,
                    Comments = filteredComments,
                    TotalCommentsScraped = scrapedData.Comments.Count,
                    TotalCommentsAfterFilter = filteredComments.Count,
                    SharersCount = filteredComments.Count(c => c.HasSharedPost),
                    GroupMembersCount = filteredComments.Count(c => c.IsGroupMember),
                    AnalyzedAt = DateTime.Now,
                    Filters = request.Filters
                };

                return Ok(new
                {
                    Success = true,
                    Data = result,
                    Message = $"Successfully analyzed {result.TotalCommentsAfterFilter} comments (filtered from {result.TotalCommentsScraped})"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing comments from URL: {Url}", request.PostUrl);
                return StatusCode(500, new 
                { 
                    Success = false,
                    Error = "Internal server error occurred while analyzing comments",
                    Details = ex.Message 
                });
            }
        }

        /// <summary>
        /// Lấy danh sách người dùng đã share post
        /// </summary>
        [HttpPost("get-sharers")]
        public async Task<ActionResult<List<ScrapedComment>>> GetSharers([FromBody] GetSharersRequest request)
        {
            try
            {
                if (!_urlParserService.IsValidFacebookPostUrl(request.PostUrl))
                {
                    return BadRequest(new { Error = "Invalid Facebook post URL" });
                }

                var options = new ScrapingOptions
                {
                    MaxCommentsToScrape = request.MaxComments ?? 1000,
                    SortOrder = SortOrder.OldestFirst,
                    UseHeadlessBrowser = true
                };

                var scrapedData = await _webScrapingService.ScrapePostCommentsAsync(request.PostUrl, options);
                var sharers = scrapedData.Comments.Where(c => c.HasSharedPost).ToList();

                return Ok(new
                {
                    Success = true,
                    Data = sharers,
                    TotalSharers = sharers.Count,
                    TotalComments = scrapedData.Comments.Count,
                    Message = $"Found {sharers.Count} commenters who shared the post"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sharers from URL: {Url}", request.PostUrl);
                return StatusCode(500, new 
                { 
                    Success = false,
                    Error = "Internal server error occurred while getting sharers",
                    Details = ex.Message 
                });
            }
        }

        private List<ScrapedComment> FilterComments(List<ScrapedComment> comments, CommentFilters? filters)
        {
            if (filters == null)
                return comments;

            var filtered = comments.AsEnumerable();

            // Filter by date range
            if (filters.StartDate.HasValue)
                filtered = filtered.Where(c => c.CommentTime >= filters.StartDate.Value);

            if (filters.EndDate.HasValue)
                filtered = filtered.Where(c => c.CommentTime <= filters.EndDate.Value);

            // Filter by minimum likes
            if (filters.MinLikes.HasValue)
                filtered = filtered.Where(c => c.LikesCount >= filters.MinLikes.Value);

            // Filter by shared status
            if (filters.OnlySharers == true)
                filtered = filtered.Where(c => c.HasSharedPost);

            // Filter by group members only
            if (filters.OnlyGroupMembers == true)
                filtered = filtered.Where(c => c.IsGroupMember);

            // Filter by content keywords
            if (!string.IsNullOrEmpty(filters.ContentKeywords))
            {
                var keywords = filters.ContentKeywords.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(k => k.Trim().ToLower());
                
                filtered = filtered.Where(c => 
                    keywords.Any(k => c.Content.ToLower().Contains(k)));
            }

            // Filter by author names
            if (!string.IsNullOrEmpty(filters.AuthorNames))
            {
                var names = filters.AuthorNames.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(n => n.Trim().ToLower());
                
                filtered = filtered.Where(c => 
                    names.Any(n => c.AuthorName.ToLower().Contains(n)));
            }

            return filtered.ToList();
        }
    }

    // Request/Response models
    public class ScrapePostRequest
    {
        public string PostUrl { get; set; } = string.Empty;
        public int? MaxComments { get; set; }
        public SortOrder? SortOrder { get; set; }
        public bool? LoadReplies { get; set; }
        public bool? LoadReactions { get; set; }
        public bool? UseHeadlessBrowser { get; set; }
        public int? ScrollDelayMs { get; set; }
        public int? MaxScrollAttempts { get; set; }
    }

    public class PostInfoRequest
    {
        public string PostUrl { get; set; } = string.Empty;
    }

    public class ValidateUrlRequest
    {
        public string Url { get; set; } = string.Empty;
    }

    public class AnalyzeCommentsRequest
    {
        public string PostUrl { get; set; } = string.Empty;
        public int? MaxComments { get; set; }
        public SortOrder? SortOrder { get; set; }
        public bool? LoadReplies { get; set; }
        public bool? LoadReactions { get; set; }
        public CommentFilters? Filters { get; set; }
    }

    public class GetSharersRequest
    {
        public string PostUrl { get; set; } = string.Empty;
        public int? MaxComments { get; set; }
    }

    public class CommentFilters
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MinLikes { get; set; }
        public bool? OnlySharers { get; set; }
        public bool? OnlyGroupMembers { get; set; }
        public string? ContentKeywords { get; set; }
        public string? AuthorNames { get; set; }
    }

    public class AnalyzedCommentsResult
    {
        public ScrapedPostInfo PostInfo { get; set; } = new();
        public List<ScrapedComment> Comments { get; set; } = new();
        public int TotalCommentsScraped { get; set; }
        public int TotalCommentsAfterFilter { get; set; }
        public int SharersCount { get; set; }
        public int GroupMembersCount { get; set; }
        public DateTime AnalyzedAt { get; set; }
        public CommentFilters? Filters { get; set; }
    }
}