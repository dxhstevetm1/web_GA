using FacebookCommentAnalyzer.API.Models;

namespace FacebookCommentAnalyzer.API.Services
{
	public interface IFacebookScrapeService
	{
		Task<List<FacebookComment>> ScrapeCommentsAsync(string postUrl, bool checkShare, CancellationToken cancellationToken = default);
	}
}
