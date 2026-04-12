using Archive.Web.ViewModels.Feed;
using Archive.Web.ViewModels.Shared;

namespace Archive.Web.Services;

public interface IFeedService
{
    Task<LandingPageViewModel> GetLandingPageAsync(int? currentUserId);
    Task<FeedPageViewModel> GetFeedAsync(int userId);
    Task<List<PostCardViewModel>> LoadMorePostsAsync(int userId, int skip, int take);
    Task<PostDetailPageViewModel?> GetPostDetailAsync(int postId, int? currentUserId);
}
