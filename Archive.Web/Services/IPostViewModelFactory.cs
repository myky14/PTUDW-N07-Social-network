using Archive.Web.Models;
using Archive.Web.ViewModels.Shared;

namespace Archive.Web.Services;

public interface IPostViewModelFactory
{
    Task<PostCardViewModel?> BuildPostCardAsync(int postId, int? currentUserId);
    List<PostCardViewModel> BuildPostCards(IEnumerable<Post> posts, int? currentUserId);
}
