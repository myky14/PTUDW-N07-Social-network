using System.Security.Claims;
using Archive.Web.Models;
using Archive.Web.Services;
using Archive.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Archive.Web.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Feed");
        }

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.ValidateUserAsync(model);
        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        await SignInUserAsync(result.Data);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return result.Data.Role?.Name == "Admin"
            ? RedirectToAction("Index", "Dashboard", new { area = "Admin" })
            : RedirectToAction("Index", "Feed");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Feed");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.RegisterAsync(model);
        if (!result.Success || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        await SignInUserAsync(result.Data);
        return RedirectToAction("Index", "Feed");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInUserAsync(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role?.Name ?? "User"),
            new("DisplayName", user.DisplayName)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            });
    }
}
