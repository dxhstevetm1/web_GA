using FacebookCommentAnalyzer.API.Models;
using Newtonsoft.Json;
using System.Text;

namespace FacebookCommentAnalyzer.API.Services
{
    public class FacebookService : IFacebookService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FacebookService> _logger;

        public FacebookService(HttpClient httpClient, ILogger<FacebookService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<FacebookPost?> GetPostAsync(string postId, string accessToken)
        {
            try
            {
                var url = $"https://graph.facebook.com/v18.0/{postId}?fields=id,message,created_time,updated_time,permalink_url,from&access_token={accessToken}";
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

        public async Task<List<FacebookComment>> GetAllCommentsAsync(string postId, string accessToken)
        {
            var allComments = new List<FacebookComment>();
            var nextUrl = $"https://graph.facebook.com/v18.0/{postId}/comments?fields=id,message,from,created_time,comment_count,like_count,is_hidden,can_reply&access_token={accessToken}";

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

        public async Task<bool> CheckUserSharedPost(string userId, string postUrl, string accessToken)
        {
            try
            {
                // This is a simplified check - in reality, you'd need to check the user's posts
                // for shares of the specific post URL
                var url = $"https://graph.facebook.com/v18.0/{userId}/posts?fields=message,link&access_token={accessToken}";
                var response = await _httpClient.GetAsync(url);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var postsResponse = JsonConvert.DeserializeObject<dynamic>(content);
                    
                    // Check if any post contains the original post URL
                    if (postsResponse?.data != null)
                    {
                        foreach (var post in postsResponse.data)
                        {
                            var message = post.message?.ToString() ?? "";
                            var link = post.link?.ToString() ?? "";
                            
                            if (message.Contains(postUrl) || link.Contains(postUrl))
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
                _logger.LogError(ex, "Error checking if user shared post");
                return false;
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
                    comment.HasSharedPost = await CheckUserSharedPost(comment.From.Id, post.PermalinkUrl, accessToken);
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
    }
}