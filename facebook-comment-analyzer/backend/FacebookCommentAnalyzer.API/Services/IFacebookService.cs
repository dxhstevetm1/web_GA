using FacebookCommentAnalyzer.API.Models;

namespace FacebookCommentAnalyzer.API.Services
{
    public interface IFacebookService
    {
        Task<FacebookPost?> GetPostAsync(string postId, string accessToken);
        Task<List<FacebookComment>> GetAllCommentsAsync(string postId, string accessToken);
        Task<bool> CheckUserSharedPost(string userId, string postUrl, string accessToken);
        Task<List<FacebookComment>> AnalyzeCommentsAsync(string postId, string accessToken);
    }
}