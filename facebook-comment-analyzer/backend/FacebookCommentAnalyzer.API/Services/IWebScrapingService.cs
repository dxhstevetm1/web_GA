using FacebookCommentAnalyzer.API.Models;

namespace FacebookCommentAnalyzer.API.Services
{
    public interface IWebScrapingService
    {
        Task<ScrapedPostData> ScrapePostCommentsAsync(string postUrl, ScrapingOptions? options = null);
        Task<List<ScrapedComment>> ScrapeCommentsAsync(string postUrl, ScrapingOptions? options = null);
        Task<bool> CheckUserSharedPostAsync(string postUrl, string userProfileUrl);
        Task<ScrapedPostInfo> GetPostInfoAsync(string postUrl);
    }

    public class ScrapingOptions
    {
        public int MaxCommentsToScrape { get; set; } = 1000;
        public int ScrollDelayMs { get; set; } = 2000;
        public int MaxScrollAttempts { get; set; } = 50;
        public bool LoadReplies { get; set; } = false;
        public bool LoadReactions { get; set; } = false;
        public SortOrder SortOrder { get; set; } = SortOrder.OldestFirst;
        public int TimeoutMs { get; set; } = 60000;
        public bool UseHeadlessBrowser { get; set; } = true;
    }

    public enum SortOrder
    {
        OldestFirst,
        NewestFirst,
        MostRelevant
    }
}