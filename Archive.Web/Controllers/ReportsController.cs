using Archive.Web.Extensions;
using Archive.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostReport(int postId, string reason, string? details)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _reportService.ReportPostAsync(userId.Value, postId, reason, details);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return Redirect(Request.Headers.Referer.ToString());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UserReport(int targetUserId, string reason, string? details)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _reportService.ReportUserAsync(userId.Value, targetUserId, reason, details);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return Redirect(Request.Headers.Referer.ToString());
    }
}
