using Archive.Web.Extensions;
using Archive.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class PostsController : Controller
{
    private readonly IAdminService _adminService;

    public PostsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _adminService.GetPostsAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleVisibility(int id)
    {
        var result = await _adminService.TogglePostVisibilityAsync(User.GetUserId()!.Value, id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
