namespace FacebookCommentAnalyzer.API.Models
{
    public class ScrapedPostData
    {
        public ScrapedPostInfo PostInfo { get; set; } = new();
        public List<ScrapedComment> Comments { get; set; } = new();
        public int TotalCommentsFound { get; set; }
        public DateTime ScrapedAt { get; set; } = DateTime.Now;
        public string SourceUrl { get; set; } = string.Empty;
    }

    public class ScrapedPostInfo
    {
        public string PostId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorProfileUrl { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public DateTime? PostTime { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public int SharesCount { get; set; }
        public string PostUrl { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public bool IsGroupPost { get; set; }
        public List<string> PostImages { get; set; } = new();
    }

    public class ScrapedComment
    {
        public string CommentId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorProfileUrl { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public DateTime? CommentTime { get; set; }
        public int LikesCount { get; set; }
        public int RepliesCount { get; set; }
        public bool HasSharedPost { get; set; }
        public string ShareUrl { get; set; } = string.Empty;
        public DateTime? ShareTime { get; set; }
        public string ShareType { get; set; } = string.Empty;
        public List<ScrapedComment> Replies { get; set; } = new();
        public bool IsGroupMember { get; set; }
        public string GroupRole { get; set; } = string.Empty;
        public string CommentUrl { get; set; } = string.Empty;
        public List<string> CommentImages { get; set; } = new();
        public List<ReactionData> Reactions { get; set; } = new();
    }

    public class ReactionData
    {
        public string Type { get; set; } = string.Empty; // like, love, haha, wow, sad, angry
        public int Count { get; set; }
    }

    public class UserShareInfo
    {
        public bool HasShared { get; set; }
        public string ShareUrl { get; set; } = string.Empty;
        public DateTime? ShareTime { get; set; }
        public string ShareContent { get; set; } = string.Empty;
        public string ShareType { get; set; } = string.Empty; // public, friends, etc.
    }
}