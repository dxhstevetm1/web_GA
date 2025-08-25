using FacebookCommentAnalyzer.API.Models;

namespace FacebookCommentAnalyzer.API.Services
{
    public interface IUrlParserService
    {
        FacebookUrlInfo ParseFacebookUrl(string url);
        bool IsValidFacebookPostUrl(string url);
        string ExtractPostId(string url);
        string ExtractGroupId(string url);
    }

    public class FacebookUrlInfo
    {
        public string PostId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public bool IsGroupPost { get; set; }
        public bool IsPagePost { get; set; }
        public bool IsProfilePost { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string CleanUrl { get; set; } = string.Empty;
    }
}