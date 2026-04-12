using Archive.Web.Extensions;
using Archive.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IAdminService _adminService;

    public UsersController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _adminService.GetUsersAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(int id)
    {
        var result = await _adminService.ToggleUserLockAsync(User.GetUserId()!.Value, id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
