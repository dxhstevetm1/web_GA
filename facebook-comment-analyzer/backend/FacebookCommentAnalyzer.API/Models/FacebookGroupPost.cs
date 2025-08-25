using System.Text.Json.Serialization;

namespace FacebookCommentAnalyzer.API.Models
{
    public class FacebookGroupPost
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("created_time")]
        public DateTime CreatedTime { get; set; }

        [JsonPropertyName("updated_time")]
        public DateTime UpdatedTime { get; set; }

        [JsonPropertyName("permalink_url")]
        public string PermalinkUrl { get; set; } = string.Empty;

        [JsonPropertyName("from")]
        public FacebookUser From { get; set; } = new();

        [JsonPropertyName("comments")]
        public FacebookCommentsResponse Comments { get; set; } = new();

        [JsonPropertyName("likes")]
        public FacebookLikesResponse Likes { get; set; } = new();

        [JsonPropertyName("shares")]
        public int Shares { get; set; }

        [JsonPropertyName("group")]
        public FacebookGroup Group { get; set; } = new();

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("status_type")]
        public string StatusType { get; set; } = string.Empty;
    }

    public class FacebookGroup
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("privacy")]
        public string Privacy { get; set; } = string.Empty;
    }
}