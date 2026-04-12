using Archive.Web.Extensions;
using Archive.Web.Services;
using Archive.Web.ViewModels.Feed;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.Web.Controllers;

public class PostsController : Controller
{
    private readonly IPostService _postService;
    private readonly IFeedService _feedService;

    public PostsController(IPostService postService, IFeedService feedService)
    {
        _postService = postService;
        _feedService = feedService;
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PostComposerViewModel model)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage)
                .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));

            TempData["ErrorMessage"] = firstError ?? "Nội dung bài viết chưa hợp lệ.";
            return Redirect(Request.Headers.Referer.ToString());
        }

        var result = await _postService.CreatePostAsync(userId.Value, model);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

        if (result.Success && result.Data > 0 && model.QuotePostId.HasValue)
        {
            return RedirectToAction("Details", new { id = result.Data });
        }

        return Redirect(Request.Headers.Referer.ToString() == string.Empty
            ? Url.Action("Index", "Feed")!
            : Request.Headers.Referer.ToString());
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var model = await _feedService.GetPostDetailAsync(id, User.GetUserId());
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = await _postService.GetPostForEditAsync(id, userId.Value);
        if (model is null)
        {
            return NotFound();
        }

        ViewBag.PostId = id;
        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PostComposerViewModel model)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.PostId = id;
            return View(model);
        }

        var result = await _postService.UpdatePostAsync(id, userId.Value, model);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

        if (!result.Success)
        {
            ViewBag.PostId = id;
            return View(model);
        }

        return RedirectToAction("Details", new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _postService.DeletePostAsync(id, userId.Value);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Index", "Feed");
    }
}
