using Archive.Web.Extensions;
using Archive.Web.Services;
using Archive.Web.ViewModels.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.Web.Controllers;

public class ProfilesController : Controller
{
    private readonly IProfileService _profileService;

    public ProfilesController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet]
    public async Task<IActionResult> Details(string userName)
    {
        var model = await _profileService.GetProfileAsync(userName, User.GetUserId());
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = await _profileService.GetEditProfileAsync(userId.Value);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _profileService.UpdateProfileAsync(userId.Value, model);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

        if (!result.Success)
        {
            return View(model);
        }

        return RedirectToAction("Details", new { userName = User.Identity?.Name });
    }
}
