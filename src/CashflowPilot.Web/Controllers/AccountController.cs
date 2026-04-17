using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CashflowPilot.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet, AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {Email} logged in.", model.Email);
            return LocalRedirect(model.ReturnUrl ?? "/");
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
