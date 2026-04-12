using Archive.Web.Models;
using Archive.Web.ViewModels.Account;

namespace Archive.Web.Services;

public interface IAuthService
{
    Task<ServiceResult<AppUser>> RegisterAsync(RegisterViewModel model);
    Task<ServiceResult<AppUser>> ValidateUserAsync(LoginViewModel model);
}
