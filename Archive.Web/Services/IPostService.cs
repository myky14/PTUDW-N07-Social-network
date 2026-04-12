using Archive.Web.ViewModels.Feed;
using Archive.Web.ViewModels.Shared;

namespace Archive.Web.Services;

public interface IPostService
{
    Task<ServiceResult<int>> CreatePostAsync(int userId, PostComposerViewModel model);
    Task<PostComposerViewModel?> GetPostForEditAsync(int postId, int userId);
    Task<ServiceResult> UpdatePostAsync(int postId, int userId, PostComposerViewModel model);
    Task<ServiceResult> DeletePostAsync(int postId, int userId);
    Task<ServiceResult<ToggleStateViewModel>> ToggleLikeAsync(int postId, int userId);
    Task<ServiceResult<ToggleStateViewModel>> ToggleRepostAsync(int postId, int userId);
    Task<ServiceResult<CommentFeedResponseViewModel>> GetCommentsAsync(int postId);
    Task<ServiceResult<CommentFeedResponseViewModel>> AddCommentAsync(int postId, int userId, string content, int? parentCommentId);
}
