using Archive.Web.ViewModels.Profile;
using Archive.Web.ViewModels.Shared;

namespace Archive.Web.Services;

public interface IProfileService
{
    Task<ProfilePageViewModel?> GetProfileAsync(string userName, int? currentUserId);
    Task<EditProfileViewModel?> GetEditProfileAsync(int userId);
    Task<ServiceResult> UpdateProfileAsync(int userId, EditProfileViewModel model);
    Task<ServiceResult<FollowStateViewModel>> ToggleFollowAsync(int currentUserId, int targetUserId);
}
