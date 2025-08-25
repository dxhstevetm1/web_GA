using FacebookCommentAnalyzer.API.Models;
using System.Text.RegularExpressions;
using System.Web;

namespace FacebookCommentAnalyzer.API.Services
{
    public class UrlParserService : IUrlParserService
    {
        private readonly ILogger<UrlParserService> _logger;

        public UrlParserService(ILogger<UrlParserService> logger)
        {
            _logger = logger;
        }

        public FacebookUrlInfo ParseFacebookUrl(string url)
        {
            var urlInfo = new FacebookUrlInfo
            {
                OriginalUrl = url
            };

            try
            {
                if (string.IsNullOrEmpty(url))
                    return urlInfo;

                // Clean and normalize the URL
                url = CleanUrl(url);
                urlInfo.CleanUrl = url;

                // Parse different Facebook URL formats
                if (IsGroupPostUrl(url))
                {
                    urlInfo.IsGroupPost = true;
                    urlInfo.GroupId = ExtractGroupId(url);
                    urlInfo.PostId = ExtractPostId(url);
                }
                else if (IsPagePostUrl(url))
                {
                    urlInfo.IsPagePost = true;
                    urlInfo.PostId = ExtractPostId(url);
                    urlInfo.UserId = ExtractPageId(url);
                }
                else if (IsProfilePostUrl(url))
                {
                    urlInfo.IsProfilePost = true;
                    urlInfo.PostId = ExtractPostId(url);
                    urlInfo.UserId = ExtractUserId(url);
                }

                return urlInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Facebook URL: {Url}", url);
                return urlInfo;
            }
        }

        public bool IsValidFacebookPostUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            try
            {
                var uri = new Uri(url);
                
                // Check if it's a Facebook domain
                if (!IsFacebookDomain(uri.Host))
                    return false;

                // Check if it contains post patterns
                var path = uri.AbsolutePath.ToLower();
                
                return path.Contains("/posts/") || 
                       path.Contains("/permalink/") ||
                       path.Contains("/groups/") ||
                       uri.Query.Contains("story_fbid") ||
                       uri.Fragment.Contains("story_fbid");
            }
            catch
            {
                return false;
            }
        }

        public string ExtractPostId(string url)
        {
            try
            {
                // Pattern 1: /posts/123456789
                var postsMatch = Regex.Match(url, @"/posts/(\d+)", RegexOptions.IgnoreCase);
                if (postsMatch.Success)
                    return postsMatch.Groups[1].Value;

                // Pattern 2: /permalink/123456789
                var permalinkMatch = Regex.Match(url, @"/permalink/(\d+)", RegexOptions.IgnoreCase);
                if (permalinkMatch.Success)
                    return permalinkMatch.Groups[1].Value;

                // Pattern 3: story_fbid=123456789
                var storyMatch = Regex.Match(url, @"story_fbid=(\d+)", RegexOptions.IgnoreCase);
                if (storyMatch.Success)
                    return storyMatch.Groups[1].Value;

                // Pattern 4: Groups post format /groups/groupId/posts/postId or /groups/groupId/permalink/postId
                var groupPostMatch = Regex.Match(url, @"/groups/[^/]+/(?:posts|permalink)/(\d+)", RegexOptions.IgnoreCase);
                if (groupPostMatch.Success)
                    return groupPostMatch.Groups[1].Value;

                // Pattern 5: Extract from query parameters
                var uri = new Uri(url);
                var query = HttpUtility.ParseQueryString(uri.Query);
                
                if (query["story_fbid"] != null)
                    return query["story_fbid"];

                if (query["id"] != null)
                    return query["id"];

                // Pattern 6: Extract from fragment
                if (!string.IsNullOrEmpty(uri.Fragment))
                {
                    var fragmentMatch = Regex.Match(uri.Fragment, @"story_fbid=(\d+)", RegexOptions.IgnoreCase);
                    if (fragmentMatch.Success)
                        return fragmentMatch.Groups[1].Value;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting post ID from URL: {Url}", url);
                return string.Empty;
            }
        }

        public string ExtractGroupId(string url)
        {
            try
            {
                // Pattern 1: /groups/123456789/
                var groupIdMatch = Regex.Match(url, @"/groups/(\d+)", RegexOptions.IgnoreCase);
                if (groupIdMatch.Success)
                    return groupIdMatch.Groups[1].Value;

                // Pattern 2: /groups/groupname/
                var groupNameMatch = Regex.Match(url, @"/groups/([^/\?]+)", RegexOptions.IgnoreCase);
                if (groupNameMatch.Success)
                {
                    var groupName = groupNameMatch.Groups[1].Value;
                    // If it's not numeric, it's a group name that we'll need to resolve
                    if (!Regex.IsMatch(groupName, @"^\d+$"))
                        return groupName;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting group ID from URL: {Url}", url);
                return string.Empty;
            }
        }

        private string CleanUrl(string url)
        {
            // Remove mobile prefix
            url = url.Replace("m.facebook.com", "facebook.com");
            url = url.Replace("mobile.facebook.com", "facebook.com");
            
            // Remove www prefix for consistency
            url = url.Replace("www.facebook.com", "facebook.com");
            
            // Ensure https
            if (url.StartsWith("http://"))
                url = url.Replace("http://", "https://");
            
            if (!url.StartsWith("https://"))
                url = "https://" + url;

            return url;
        }

        private bool IsFacebookDomain(string host)
        {
            var facebookDomains = new[]
            {
                "facebook.com",
                "www.facebook.com",
                "m.facebook.com",
                "mobile.facebook.com",
                "fb.com"
            };

            return facebookDomains.Any(domain => 
                host.Equals(domain, StringComparison.OrdinalIgnoreCase));
        }

        private bool IsGroupPostUrl(string url)
        {
            return url.Contains("/groups/", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsPagePostUrl(string url)
        {
            // Pages typically have format: facebook.com/pagename/posts/123
            var pagePattern = @"facebook\.com/[^/]+/posts/\d+";
            return Regex.IsMatch(url, pagePattern, RegexOptions.IgnoreCase) && 
                   !url.Contains("/groups/", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsProfilePostUrl(string url)
        {
            // Profile posts: facebook.com/profile.php?id=123&story_fbid=456
            return url.Contains("profile.php", StringComparison.OrdinalIgnoreCase) ||
                   (url.Contains("story_fbid") && !IsGroupPostUrl(url) && !IsPagePostUrl(url));
        }

        private string ExtractPageId(string url)
        {
            try
            {
                // Extract page name from URL like facebook.com/pagename/posts/123
                var pageMatch = Regex.Match(url, @"facebook\.com/([^/]+)/posts/", RegexOptions.IgnoreCase);
                if (pageMatch.Success)
                    return pageMatch.Groups[1].Value;

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting page ID from URL: {Url}", url);
                return string.Empty;
            }
        }

        private string ExtractUserId(string url)
        {
            try
            {
                var uri = new Uri(url);
                var query = HttpUtility.ParseQueryString(uri.Query);
                
                if (query["id"] != null)
                    return query["id"];

                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting user ID from URL: {Url}", url);
                return string.Empty;
            }
        }
    }
}