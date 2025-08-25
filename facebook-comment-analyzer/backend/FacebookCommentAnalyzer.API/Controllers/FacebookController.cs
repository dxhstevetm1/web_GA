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

        public FacebookController(IFacebookService facebookService, ILogger<FacebookController> logger)
        {
            _facebookService = facebookService;
            _logger = logger;
        }

        [HttpGet("post/{postId}")]
        public async Task<ActionResult<FacebookPost>> GetPost(string postId, [FromQuery] string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Access token is required");
            }

            try
            {
                var post = await _facebookService.GetPostAsync(postId, accessToken);
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

        [HttpGet("post/{postId}/comments")]
        public async Task<ActionResult<List<FacebookComment>>> GetComments(string postId, [FromQuery] string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Access token is required");
            }

            try
            {
                var comments = await _facebookService.GetAllCommentsAsync(postId, accessToken);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("post/{postId}/analyze")]
        public async Task<ActionResult<List<FacebookComment>>> AnalyzeComments(string postId, [FromQuery] string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                return BadRequest("Access token is required");
            }

            try
            {
                var comments = await _facebookService.AnalyzeCommentsAsync(postId, accessToken);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing comments");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}/check-share")]
        public async Task<ActionResult<bool>> CheckUserShared(string userId, [FromQuery] string postUrl, [FromQuery] string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(postUrl))
            {
                return BadRequest("Access token and post URL are required");
            }

            try
            {
                var hasShared = await _facebookService.CheckUserSharedPost(userId, postUrl, accessToken);
                return Ok(hasShared);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking user share");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}