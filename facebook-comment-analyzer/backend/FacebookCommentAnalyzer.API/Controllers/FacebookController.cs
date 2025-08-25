using FacebookCommentAnalyzer.API.Models;
using FacebookCommentAnalyzer.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FacebookCommentAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacebookController : ControllerBase
    {
        private readonly IFacebookService _facebookService;
        private readonly ILogger<FacebookController> _logger;
        private readonly FacebookApiConfig _config;

        public FacebookController(IFacebookService facebookService, ILogger<FacebookController> logger, FacebookApiConfig config)
        {
            _facebookService = facebookService;
            _logger = logger;
            _config = config;
        }

        [HttpGet("post/{postId}")]
        public async Task<ActionResult<FacebookPost>> GetPost(string postId, [FromQuery] string? accessToken = null)
        {
            var token = accessToken ?? _config.AccessToken;
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Access token is required");
            }

            try
            {
                var post = await _facebookService.GetPostAsync(postId, token);
                if (post == null)
                {
                    return NotFound("Post not found or access denied");
                }

                return Ok(post);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting post");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("group-post/{postId}")]
        public async Task<ActionResult<FacebookGroupPost>> GetGroupPost(string postId, [FromQuery] string? accessToken = null)
        {
            var token = accessToken ?? _config.AccessToken;
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Access token is required");
            }

            try
            {
                var groupPost = await _facebookService.GetGroupPostAsync(postId, token);
                if (groupPost == null)
                {
                    return NotFound("Group post not found or access denied");
                }

                return Ok(groupPost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group post");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("post/{postId}/comments")]
        public async Task<ActionResult<List<FacebookComment>>> GetComments(string postId, [FromQuery] string? accessToken = null)
        {
            var token = accessToken ?? _config.AccessToken;
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Access token is required");
            }

            try
            {
                var comments = await _facebookService.GetAllCommentsAsync(postId, token);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("group-post/{postId}/comments")]
        public async Task<ActionResult<List<FacebookComment>>> GetGroupPostComments(string postId, [FromQuery] string? accessToken = null)
        {
            var token = accessToken ?? _config.AccessToken;
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Access token is required");
            }

            try
            {
                var comments = await _facebookService.GetGroupPostCommentsAsync(postId, token);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting group post comments");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("post/{postId}/analyze")]
        public async Task<ActionResult<List<FacebookComment>>> AnalyzeComments(string postId, [FromQuery] string? accessToken = null)
        {
            var token = accessToken ?? _config.AccessToken;
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Access token is required");
            }

            try
            {
                var comments = await _facebookService.AnalyzeCommentsAsync(postId, token);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing comments");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("group-post/{postId}/analyze")]
        public async Task<ActionResult<List<FacebookComment>>> AnalyzeGroupPostComments(string postId, [FromQuery] string? accessToken = null)
        {
            var token = accessToken ?? _config.AccessToken;
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Access token is required");
            }

            try
            {
                var comments = await _facebookService.AnalyzeGroupPostCommentsAsync(postId, token);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing group post comments");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}/check-share")]
        public async Task<ActionResult<bool>> CheckUserShared(string userId, [FromQuery] string postUrl, [FromQuery] string? accessToken = null)
        {
            var token = accessToken ?? _config.AccessToken;
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(postUrl))
            {
                return BadRequest("Access token and post URL are required");
            }

            try
            {
                var hasShared = await _facebookService.CheckUserSharedPost(userId, postUrl, token);
                return Ok(hasShared);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user share");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}/share-analysis")]
        public async Task<ActionResult<ShareAnalysisResult>> AnalyzeUserShare(string userId, [FromQuery] string postUrl, [FromQuery] string? accessToken = null)
        {
            var token = accessToken ?? _config.AccessToken;
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(postUrl))
            {
                return BadRequest("Access token and post URL are required");
            }

            try
            {
                var shareAnalysis = await _facebookService.AnalyzeUserShareActivity(userId, postUrl, token);
                return Ok(shareAnalysis);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing user share");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("group/{groupId}/member/{userId}")]
        public async Task<ActionResult<object>> GetUserGroupInfo(string groupId, string userId, [FromQuery] string? accessToken = null)
        {
            var token = accessToken ?? _config.AccessToken;
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Access token is required");
            }

            try
            {
                var isMember = await _facebookService.IsUserGroupMember(userId, groupId, token);
                var role = await _facebookService.GetUserGroupRole(userId, groupId, token);

                return Ok(new
                {
                    UserId = userId,
                    GroupId = groupId,
                    IsMember = isMember,
                    Role = role
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user group info");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("config")]
        public ActionResult<object> GetConfig()
        {
            return Ok(new
            {
                BaseUrl = _config.BaseUrl,
                HasAccessToken = !string.IsNullOrEmpty(_config.AccessToken),
                HasAppId = !string.IsNullOrEmpty(_config.AppId)
            });
        }
    }
}