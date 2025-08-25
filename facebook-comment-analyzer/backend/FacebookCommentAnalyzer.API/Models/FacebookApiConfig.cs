namespace FacebookCommentAnalyzer.API.Models
{
    public class FacebookApiConfig
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string DefaultFields { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        public string AppSecret { get; set; } = string.Empty;
    }
}