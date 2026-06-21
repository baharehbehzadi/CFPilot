using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Web.Controllers;

[Authorize]
public class BillingController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IBillingService _billing;
    private readonly IAuditService _audit;
    private readonly ILogger<BillingController> _logger;

    public BillingController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IBillingService billing, IAuditService audit, ILogger<BillingController> logger)
    {
        _db = db;
        _userManager = userManager;
        _billing = billing;
        _audit = audit;
        _logger = logger;
    }

    private async Task<(ApplicationUser user, int orgId)> GetUserAsync()
    {
        var user = await _userManager.GetUserAsync(User) ?? throw new InvalidOperationException("User not found");
        return (user, user.OrganizationId);
    }

    public async Task<IActionResult> Index()
    {
        var (_, orgId) = await GetUserAsync();
        var org = await _db.Organizations.FindAsync(orgId) ?? throw new InvalidOperationException("Organization not found");
        return View(new BillingViewModel
        {
            OrganizationName = org.Name,
            PlanTier = org.PlanTier,
            SubscriptionStatus = org.SubscriptionStatus,
            TrialEndsAt = org.TrialEndsAt,
            CurrentPeriodEnd = org.CurrentPeriodEnd,
            HasStripeCustomer = !string.IsNullOrEmpty(org.StripeCustomerId)
        });
    }

    [Authorize(Policy = "RequireAdmin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe(string plan)
    {
        var (user, orgId) = await GetUserAsync();
        var successUrl = Url.Action("SubscribeSuccess", "Billing", null, Request.Scheme)!;
        var cancelUrl = Url.Action("Index", "Billing", null, Request.Scheme)!;

        try
        {
            var checkoutUrl = await _billing.CreateCheckoutSessionAsync(orgId, plan, successUrl, cancelUrl);
            await _audit.LogAsync(orgId, user.Id, user.DisplayName, "BillingCheckoutStarted", "Organization", orgId.ToString(), $"Started checkout for plan {plan}");
            return Redirect(checkoutUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Stripe checkout session for organization {OrgId}", orgId);
            TempData["Error"] = "Could not start checkout. Please try again or contact support.";
            return RedirectToAction("Index");
        }
    }

    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> ManagePortal()
    {
        var (_, orgId) = await GetUserAsync();
        var returnUrl = Url.Action("Index", "Billing", null, Request.Scheme)!;

        try
        {
            var portalUrl = await _billing.CreateBillingPortalSessionAsync(orgId, returnUrl);
            return Redirect(portalUrl);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index");
        }
    }

    public IActionResult SubscribeSuccess()
    {
        TempData["Success"] = "Subscription activated! It may take a few seconds for your plan to update.";
        return RedirectToAction("Index");
    }
}
