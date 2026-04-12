using Archive.Web.ViewModels.Shared;

namespace Archive.Web.ViewModels.Profile;

public class ProfilePageViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsCurrentUser { get; set; }
    public bool IsFollowing { get; set; }
    public bool IsLocked { get; set; }
    public int PostCount { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public List<PostCardViewModel> Posts { get; set; } = new();
}
