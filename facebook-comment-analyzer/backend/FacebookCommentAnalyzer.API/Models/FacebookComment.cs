using System.Text.Json.Serialization;

namespace FacebookCommentAnalyzer.API.Models
{
    public class FacebookComment
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("from")]
        public FacebookUser From { get; set; } = new();

        [JsonPropertyName("created_time")]
        public DateTime CreatedTime { get; set; }

        [JsonPropertyName("comment_count")]
        public int CommentCount { get; set; }

        [JsonPropertyName("like_count")]
        public int LikeCount { get; set; }

        [JsonPropertyName("is_hidden")]
        public bool IsHidden { get; set; }

        [JsonPropertyName("can_reply")]
        public bool CanReply { get; set; }

        // Custom properties for our analysis
        public bool HasSharedPost { get; set; }
        public string ShareUrl { get; set; } = string.Empty;
        public DateTime AnalysisTime { get; set; } = DateTime.Now;
        public string ShareType { get; set; } = string.Empty; // "public", "friends", "private"
        public string ShareMessage { get; set; } = string.Empty;
        public DateTime? ShareTime { get; set; }
        public bool IsGroupMember { get; set; }
        public string GroupRole { get; set; } = string.Empty; // "admin", "moderator", "member"
    }

    public class FacebookUser
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("picture")]
        public FacebookPicture Picture { get; set; } = new();

        // Scraping-only convenience: user's profile URL if available
        public string ProfileUrl { get; set; } = string.Empty;
    }

    public class FacebookPicture
    {
        [JsonPropertyName("data")]
        public FacebookPictureData Data { get; set; } = new();
    }

    public class FacebookPictureData
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("is_silhouette")]
        public bool IsSilhouette { get; set; }
    }
}