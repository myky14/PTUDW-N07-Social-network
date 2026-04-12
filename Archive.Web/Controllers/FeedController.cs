using Archive.Web.Extensions;
using Archive.Web.Services;
using Archive.Web.ViewModels.Feed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.Web.Controllers;

[Authorize]
public class FeedController : Controller
{
    private readonly IFeedService _feedService;

    public FeedController(IFeedService feedService)
    {
        _feedService = feedService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = await _feedService.GetFeedAsync(userId.Value);
        return View(model);
    }
}
