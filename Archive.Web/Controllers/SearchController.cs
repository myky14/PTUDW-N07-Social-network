using Archive.Web.Extensions;
using Archive.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Archive.Web.Controllers;

public class SearchController : Controller
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<IActionResult> Index(string? q)
    {
        var model = await _searchService.SearchAsync(q, User.GetUserId());
        return View(model);
    }
}
