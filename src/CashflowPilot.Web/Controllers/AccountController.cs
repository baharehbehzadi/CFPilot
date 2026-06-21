using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Entities;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Web.Controllers;

public class AccountController : Controller
{
    private const int TrialLengthDays = 14;

    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ApplicationDbContext db, IAuditService audit, ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
        _audit = audit;
        _logger = logger;
    }

    [HttpGet, AllowAnonymous]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var companyName = model.CompanyName.Trim();
        if (await _db.Organizations.AnyAsync(o => o.Name.ToLower() == companyName.ToLower()))
        {
            ModelState.AddModelError(nameof(model.CompanyName), "An organization with this name is already registered.");
            return View(model);
        }

        if (await _userManager.FindByEmailAsync(model.Email) != null)
        {
            ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
            return View(model);
        }

        using var transaction = await _db.Database.BeginTransactionAsync();

        var org = new Organization
        {
            Name = companyName,
            CreatedAt = DateTime.UtcNow,
            PlanTier = PlanTier.Trial,
            SubscriptionStatus = SubscriptionStatus.Trialing,
            TrialEndsAt = DateTime.UtcNow.AddDays(TrialLengthDays)
        };
        _db.Organizations.Add(org);
        await _db.SaveChangesAsync();

        _db.OrganizationSettings.Add(new OrganizationSettings { OrganizationId = org.Id });

        var user = new ApplicationUser
        {
            UserName = model.Email, Email = model.Email, EmailConfirmed = true,
            DisplayName = model.DisplayName, OrganizationId = org.Id, CreatedAt = DateTime.UtcNow
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }
        await _userManager.AddToRoleAsync(user, Roles.Admin);
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        await _audit.LogAsync(org.Id, user.Id, user.DisplayName, "OrganizationSignUp", "Organization", org.Id.ToString(),
            $"New organization '{org.Name}' signed up with admin {model.Email}; {TrialLengthDays}-day trial started.", HttpContext.Connection.RemoteIpAddress?.ToString());

        await _signInManager.SignInAsync(user, isPersistent: false);
        TempData["Success"] = $"Welcome to CashflowPilot! Your {TrialLengthDays}-day trial has started.";
        return RedirectToAction("Index", "Dashboard");
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
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                await _audit.LogAsync(user.OrganizationId, user.Id, user.DisplayName, "Login", "User", user.Id, $"User logged in: {model.Email}", HttpContext.Connection.RemoteIpAddress?.ToString());
            }
            return LocalRedirect(model.ReturnUrl ?? "/");
        }

        var failedUser = await _userManager.FindByEmailAsync(model.Email);
        if (failedUser != null)
        {
            await _audit.LogAsync(failedUser.OrganizationId, failedUser.Id, failedUser.DisplayName, "LoginFailed", "User", failedUser.Id, $"Failed login attempt: {model.Email}", HttpContext.Connection.RemoteIpAddress?.ToString());
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            await _audit.LogAsync(user.OrganizationId, user.Id, user.DisplayName, "Logout", "User", user.Id, $"User logged out: {user.Email}", HttpContext.Connection.RemoteIpAddress?.ToString());
        }
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    public IActionResult AccessDenied() => View();
}
