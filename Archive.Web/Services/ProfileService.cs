using Archive.Web.Data;
using Archive.Web.ViewModels.Profile;
using Archive.Web.ViewModels.Shared;
using Microsoft.EntityFrameworkCore;

namespace Archive.Web.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;
    private readonly IPostViewModelFactory _postViewModelFactory;

    public ProfileService(
        AppDbContext dbContext,
        IFileStorageService fileStorageService,
        IPostViewModelFactory postViewModelFactory)
    {
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
        _postViewModelFactory = postViewModelFactory;
    }

    public async Task<ProfilePageViewModel?> GetProfileAsync(string userName, int? currentUserId)
    {
        var profileUser = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserName == userName.ToLower());

        if (profileUser is null)
        {
            return null;
        }

        var posts = await _dbContext.Posts
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Topic)
            .Include(x => x.Images)
            .Include(x => x.Likes)
            .Include(x => x.Comments)
            .Include(x => x.Reposts)
            .Include(x => x.PostHashtags)
                .ThenInclude(x => x.Hashtag)
            .Include(x => x.QuotePost)
                .ThenInclude(x => x!.User)
            .Include(x => x.QuotePost)
                .ThenInclude(x => x!.Images)
            .Where(x => x.UserId == profileUser.Id && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var followersCount = await _dbContext.Follows.CountAsync(x => x.FollowingId == profileUser.Id);
        var followingCount = await _dbContext.Follows.CountAsync(x => x.FollowerId == profileUser.Id);

        return new ProfilePageViewModel
        {
            Id = profileUser.Id,
            UserName = profileUser.UserName,
            DisplayName = profileUser.DisplayName,
            AvatarUrl = profileUser.AvatarUrl,
            Bio = profileUser.Bio,
            IsCurrentUser = currentUserId == profileUser.Id,
            IsFollowing = currentUserId.HasValue && await _dbContext.Follows.AnyAsync(x => x.FollowerId == currentUserId.Value && x.FollowingId == profileUser.Id),
            IsLocked = profileUser.IsLocked,
            PostCount = posts.Count,
            FollowersCount = followersCount,
            FollowingCount = followingCount,
            Posts = _postViewModelFactory.BuildPostCards(posts, currentUserId)
        };
    }

    public async Task<EditProfileViewModel?> GetEditProfileAsync(int userId)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user is null)
        {
            return null;
        }

        return new EditProfileViewModel
        {
            DisplayName = user.DisplayName,
            Bio = user.Bio,
            CurrentAvatarUrl = user.AvatarUrl
        };
    }

    public async Task<ServiceResult> UpdateProfileAsync(int userId, EditProfileViewModel model)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user is null)
        {
            return ServiceResult.Fail("Không tìm thấy người dùng.");
        }

        user.DisplayName = model.DisplayName.Trim();
        user.Bio = string.IsNullOrWhiteSpace(model.Bio) ? null : model.Bio.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        if (model.AvatarFile is not null)
        {
            var uploadResult = await _fileStorageService.SaveImageAsync(model.AvatarFile, "avatars");
            if (!uploadResult.Success || string.IsNullOrWhiteSpace(uploadResult.Data))
            {
                return ServiceResult.Fail(uploadResult.Message);
            }

            user.AvatarUrl = uploadResult.Data;
        }

        await _dbContext.SaveChangesAsync();
        return ServiceResult.Ok("Cập nhật hồ sơ thành công.");
    }

    public async Task<ServiceResult<FollowStateViewModel>> ToggleFollowAsync(int currentUserId, int targetUserId)
    {
        if (currentUserId == targetUserId)
        {
            return ServiceResult<FollowStateViewModel>.Fail("Bạn không thể tự theo dõi chính mình.");
        }

        var targetExists = await _dbContext.Users.AnyAsync(x => x.Id == targetUserId && x.IsActive && !x.IsLocked);
        if (!targetExists)
        {
            return ServiceResult<FollowStateViewModel>.Fail("Người dùng không tồn tại.");
        }

        var follow = await _dbContext.Follows.FirstOrDefaultAsync(x => x.FollowerId == currentUserId && x.FollowingId == targetUserId);
        var active = false;

        if (follow is null)
        {
            _dbContext.Follows.Add(new Models.Follow
            {
                FollowerId = currentUserId,
                FollowingId = targetUserId
            });
            active = true;
        }
        else
        {
            _dbContext.Follows.Remove(follow);
        }

        await _dbContext.SaveChangesAsync();

        return ServiceResult<FollowStateViewModel>.Ok(new FollowStateViewModel
        {
            Active = active,
            FollowersCount = await _dbContext.Follows.CountAsync(x => x.FollowingId == targetUserId)
        }, active ? "Đã theo dõi tài khoản." : "Đã bỏ theo dõi.");
    }
}
