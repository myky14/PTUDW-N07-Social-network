using Archive.Web.Data;
using Archive.Web.Models;
using Archive.Web.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Archive.Web.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public AuthService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ServiceResult<AppUser>> RegisterAsync(RegisterViewModel model)
    {
        if (await _dbContext.Users.AnyAsync(x => x.Email == model.Email.Trim().ToLower()))
        {
            return ServiceResult<AppUser>.Fail("Email đã được sử dụng.");
        }

        if (await _dbContext.Users.AnyAsync(x => x.UserName == model.UserName.Trim().ToLower()))
        {
            return ServiceResult<AppUser>.Fail("Username đã tồn tại.");
        }

        var userRole = await _dbContext.Roles.FirstAsync(x => x.Name == "User");
        var user = new AppUser
        {
            RoleId = userRole.Id,
            UserName = model.UserName.Trim().ToLower(),
            DisplayName = model.DisplayName.Trim(),
            Email = model.Email.Trim().ToLower(),
            Bio = "Mới gia nhập ARCHIVE, đang gom lại những cảm xúc nhỏ trong ngày.",
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        user.Role = userRole;

        return ServiceResult<AppUser>.Ok(user, "Tạo tài khoản thành công.");
    }

    public async Task<ServiceResult<AppUser>> ValidateUserAsync(LoginViewModel model)
    {
        var email = model.Email.Trim().ToLower();
        var user = await _dbContext.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user is null)
        {
            return ServiceResult<AppUser>.Fail("Không tìm thấy tài khoản.");
        }

        if (user.IsLocked || !user.IsActive)
        {
            return ServiceResult<AppUser>.Fail("Tài khoản hiện đang bị khóa.");
        }

        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (verification == PasswordVerificationResult.Failed)
        {
            return ServiceResult<AppUser>.Fail("Mật khẩu chưa đúng.");
        }

        return ServiceResult<AppUser>.Ok(user, "Đăng nhập thành công.");
    }
}
