using System.Text.Json.Serialization;

namespace FacebookCommentAnalyzer.API.Models
{
    public class FacebookPost
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

        [JsonPropertyName("comments")]
        public FacebookCommentsResponse Comments { get; set; } = new();

        [JsonPropertyName("likes")]
        public FacebookLikesResponse Likes { get; set; } = new();

        [JsonPropertyName("shares")]
        public int Shares { get; set; }

        [JsonPropertyName("from")]
        public FacebookUser From { get; set; } = new();
    }

    public class FacebookCommentsResponse
    {
        [JsonPropertyName("data")]
        public List<FacebookComment> Data { get; set; } = new();

        [JsonPropertyName("paging")]
        public FacebookPaging Paging { get; set; } = new();
    }

    public class FacebookLikesResponse
    {
        [JsonPropertyName("data")]
        public List<FacebookUser> Data { get; set; } = new();

        [JsonPropertyName("paging")]
        public FacebookPaging Paging { get; set; } = new();
    }

    public class FacebookPaging
    {
        [JsonPropertyName("cursors")]
        public FacebookCursors Cursors { get; set; } = new();

        [JsonPropertyName("next")]
        public string? Next { get; set; }

        [JsonPropertyName("previous")]
        public string? Previous { get; set; }
    }

    public class FacebookCursors
    {
        [JsonPropertyName("before")]
        public string? Before { get; set; }

        [JsonPropertyName("after")]
        public string? After { get; set; }
    }
}