using Archive.Web.Services;
using Archive.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TopicsController : Controller
{
    private readonly IAdminService _adminService;

    public TopicsController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.TopicForm = new EditTopicViewModel();
        var model = await _adminService.GetTopicsAsync();
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _adminService.GetTopicEditModelAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        ViewBag.IsEditMode = true;
        return View("Edit", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(EditTopicViewModel model)
    {
        if (!ModelState.IsValid)
        {
            if (model.Id.HasValue)
            {
                return View("Edit", model);
            }

            ViewBag.TopicForm = model;
            var topics = await _adminService.GetTopicsAsync();
            return View("Index", topics);
        }

        var result = await _adminService.SaveTopicAsync(model);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;

        if (!result.Success && model.Id.HasValue)
        {
            return View("Edit", model);
        }

        if (!result.Success)
        {
            ViewBag.TopicForm = model;
            var topics = await _adminService.GetTopicsAsync();
            return View("Index", topics);
        }

        return RedirectToAction(nameof(Index));
    }
}
