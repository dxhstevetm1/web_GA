using FacebookCommentAnalyzer.API.Models;

namespace FacebookCommentAnalyzer.API.Services
{
    public interface IFacebookService
    {
        Task<FacebookPost?> GetPostAsync(string postId, string accessToken);
        Task<FacebookGroupPost?> GetGroupPostAsync(string postId, string accessToken);
        Task<List<FacebookComment>> GetAllCommentsAsync(string postId, string accessToken);
        Task<List<FacebookComment>> GetGroupPostCommentsAsync(string postId, string accessToken);
        Task<bool> CheckUserSharedPost(string userId, string postUrl, string accessToken);
        Task<ShareAnalysisResult> AnalyzeUserShareActivity(string userId, string postUrl, string accessToken);
        Task<List<FacebookComment>> AnalyzeCommentsAsync(string postId, string accessToken);
        Task<List<FacebookComment>> AnalyzeGroupPostCommentsAsync(string postId, string accessToken);
        Task<bool> IsUserGroupMember(string userId, string groupId, string accessToken);
        Task<string> GetUserGroupRole(string userId, string groupId, string accessToken);
    }

    public class ShareAnalysisResult
    {
        public bool HasShared { get; set; }
        public string ShareUrl { get; set; } = string.Empty;
        public string ShareType { get; set; } = string.Empty;
        public string ShareMessage { get; set; } = string.Empty;
        public DateTime? ShareTime { get; set; }
        public int ShareLikes { get; set; }
        public int ShareComments { get; set; }
    }
}