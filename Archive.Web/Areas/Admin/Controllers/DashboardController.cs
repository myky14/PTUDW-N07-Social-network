using Archive.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IAdminService _adminService;

    public DashboardController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _adminService.GetDashboardAsync();
        return View(model);
    }
}
