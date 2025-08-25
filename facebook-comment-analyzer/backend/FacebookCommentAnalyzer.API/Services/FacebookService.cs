using FacebookCommentAnalyzer.API.Models;
using Newtonsoft.Json;
using System.Text;

namespace FacebookCommentAnalyzer.API.Services
{
    public class FacebookService : IFacebookService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FacebookService> _logger;
        private readonly FacebookApiConfig _config;

        public FacebookService(HttpClient httpClient, ILogger<FacebookService> logger, FacebookApiConfig config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
        }

        public async Task<FacebookPost?> GetPostAsync(string postId, string accessToken)
        {
            try
            {
                var url = $"{_config.BaseUrl}/{postId}?fields=id,message,created_time,updated_time,permalink_url,from,likes,shares&access_token={accessToken}";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<FacebookPost>(content);
                }
                
                _logger.LogError($"Failed to get post: {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Facebook post");
                return null;
            }
        }

        public async Task<FacebookGroupPost?> GetGroupPostAsync(string postId, string accessToken)
        {
            try
            {
                var url = $"{_config.BaseUrl}/{postId}?fields=id,message,created_time,updated_time,permalink_url,from,likes,shares,group,type,status_type&access_token={accessToken}";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<FacebookGroupPost>(content);
                }
                
                _logger.LogError($"Failed to get group post: {response.StatusCode} - {response.ReasonPhrase}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Facebook group post");
                return null;
            }
        }

        public async Task<List<FacebookComment>> GetAllCommentsAsync(string postId, string accessToken)
        {
            var allComments = new List<FacebookComment>();
            var nextUrl = $"{_config.BaseUrl}/{postId}/comments?fields={_config.DefaultFields}&access_token={accessToken}";

            try
            {
                while (!string.IsNullOrEmpty(nextUrl))
                {
                    var response = await _httpClient.GetAsync(nextUrl);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var commentsResponse = JsonConvert.DeserializeObject<FacebookCommentsResponse>(content);
                        
                        if (commentsResponse?.Data != null)
                        {
                            allComments.AddRange(commentsResponse.Data);
                        }
                        
                        nextUrl = commentsResponse?.Paging?.Next;
                    }
                    else
                    {
                        _logger.LogError($"Failed to get comments: {response.StatusCode} - {response.ReasonPhrase}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Facebook comments");
            }

            return allComments.OrderBy(c => c.CreatedTime).ToList();
        }

        public async Task<List<FacebookComment>> GetGroupPostCommentsAsync(string postId, string accessToken)
        {
            var allComments = new List<FacebookComment>();
            var nextUrl = $"{_config.BaseUrl}/{postId}/comments?fields={_config.DefaultFields}&access_token={accessToken}";

            try
            {
                while (!string.IsNullOrEmpty(nextUrl))
                {
                    var response = await _httpClient.GetAsync(nextUrl);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var commentsResponse = JsonConvert.DeserializeObject<FacebookCommentsResponse>(content);
                        
                        if (commentsResponse?.Data != null)
                        {
                            // Get group info for each comment
                            foreach (var comment in commentsResponse.Data)
                            {
                                var groupPost = await GetGroupPostAsync(postId, accessToken);
                                if (groupPost?.Group != null)
                                {
                                    comment.IsGroupMember = await IsUserGroupMember(comment.From.Id, groupPost.Group.Id, accessToken);
                                    comment.GroupRole = await GetUserGroupRole(comment.From.Id, groupPost.Group.Id, accessToken);
                                }
                            }
                            
                            allComments.AddRange(commentsResponse.Data);
                        }
                        
                        nextUrl = commentsResponse?.Paging?.Next;
                    }
                    else
                    {
                        _logger.LogError($"Failed to get group post comments: {response.StatusCode} - {response.ReasonPhrase}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Facebook group post comments");
            }

            return allComments.OrderBy(c => c.CreatedTime).ToList();
        }

        public async Task<bool> CheckUserSharedPost(string userId, string postUrl, string accessToken)
        {
            try
            {
                var shareAnalysis = await AnalyzeUserShareActivity(userId, postUrl, accessToken);
                return shareAnalysis.HasShared;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user shared post");
                return false;
            }
        }

        public async Task<ShareAnalysisResult> AnalyzeUserShareActivity(string userId, string postUrl, string accessToken)
        {
            var result = new ShareAnalysisResult();
            
            try
            {
                // Get user's posts and check for shares
                var url = $"{_config.BaseUrl}/{userId}/posts?fields=id,message,link,created_time,likes,comments,privacy&access_token={accessToken}";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var postsResponse = JsonConvert.DeserializeObject<dynamic>(content);
                    
                    if (postsResponse?.data != null)
                    {
                        foreach (var post in postsResponse.data)
                        {
                            var message = post.message?.ToString() ?? "";
                            var link = post.link?.ToString() ?? "";
                            var privacy = post.privacy?.value?.ToString() ?? "public";
                            
                            // Check if this post contains the original post URL
                            if (message.Contains(postUrl) || link.Contains(postUrl))
                            {
                                result.HasShared = true;
                                result.ShareUrl = post.permalink_url?.ToString() ?? "";
                                result.ShareType = privacy;
                                result.ShareMessage = message;
                                result.ShareTime = DateTime.TryParse(post.created_time?.ToString(), out var shareTime) ? shareTime : null;
                                result.ShareLikes = post.likes?.data?.Count ?? 0;
                                result.ShareComments = post.comments?.data?.Count ?? 0;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing user share activity");
            }
            
            return result;
        }

        public async Task<bool> IsUserGroupMember(string userId, string groupId, string accessToken)
        {
            try
            {
                var url = $"{_config.BaseUrl}/{groupId}/members?access_token={accessToken}";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var membersResponse = JsonConvert.DeserializeObject<dynamic>(content);
                    
                    if (membersResponse?.data != null)
                    {
                        foreach (var member in membersResponse.data)
                        {
                            if (member.id?.ToString() == userId)
                            {
                                return true;
                            }
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user is group member");
                return false;
            }
        }

        public async Task<string> GetUserGroupRole(string userId, string groupId, string accessToken)
        {
            try
            {
                var url = $"{_config.BaseUrl}/{groupId}/members?fields=administrator&access_token={accessToken}";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var membersResponse = JsonConvert.DeserializeObject<dynamic>(content);
                    
                    if (membersResponse?.data != null)
                    {
                        foreach (var member in membersResponse.data)
                        {
                            if (member.id?.ToString() == userId)
                            {
                                if (member.administrator == true)
                                {
                                    return "admin";
                                }
                                else if (member.moderator == true)
                                {
                                    return "moderator";
                                }
                                else
                                {
                                    return "member";
                                }
                            }
                        }
                    }
                }
                
                return "member";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user group role");
                return "member";
            }
        }

        public async Task<List<FacebookComment>> AnalyzeCommentsAsync(string postId, string accessToken)
        {
            var comments = await GetAllCommentsAsync(postId, accessToken);
            var post = await GetPostAsync(postId, accessToken);
            
            if (post == null)
            {
                return comments;
            }

            // Analyze each comment to check if user shared the post
            foreach (var comment in comments)
            {
                try
                {
                    var shareAnalysis = await AnalyzeUserShareActivity(comment.From.Id, post.PermalinkUrl, accessToken);
                    comment.HasSharedPost = shareAnalysis.HasShared;
                    comment.ShareUrl = shareAnalysis.ShareUrl;
                    comment.ShareType = shareAnalysis.ShareType;
                    comment.ShareMessage = shareAnalysis.ShareMessage;
                    comment.ShareTime = shareAnalysis.ShareTime;
                    comment.AnalysisTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error analyzing comment {comment.Id}");
                    comment.HasSharedPost = false;
                }
            }

            return comments.OrderBy(c => c.CreatedTime).ToList();
        }

        public async Task<List<FacebookComment>> AnalyzeGroupPostCommentsAsync(string postId, string accessToken)
        {
            var comments = await GetGroupPostCommentsAsync(postId, accessToken);
            var groupPost = await GetGroupPostAsync(postId, accessToken);
            
            if (groupPost == null)
            {
                return comments;
            }

            // Analyze each comment to check if user shared the post
            foreach (var comment in comments)
            {
                try
                {
                    var shareAnalysis = await AnalyzeUserShareActivity(comment.From.Id, groupPost.PermalinkUrl, accessToken);
                    comment.HasSharedPost = shareAnalysis.HasShared;
                    comment.ShareUrl = shareAnalysis.ShareUrl;
                    comment.ShareType = shareAnalysis.ShareType;
                    comment.ShareMessage = shareAnalysis.ShareMessage;
                    comment.ShareTime = shareAnalysis.ShareTime;
                    comment.AnalysisTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error analyzing group post comment {comment.Id}");
                    comment.HasSharedPost = false;
                }
            }

            return comments.OrderBy(c => c.CreatedTime).ToList();
        }
    }
}