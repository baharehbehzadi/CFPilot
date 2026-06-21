using System.Security.Claims;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Web.Filters;

/// Blocks access to the app once an organization's trial has expired or its subscription has lapsed,
/// redirecting to Billing so an admin can subscribe instead of every controller re-checking this itself.
public class SubscriptionGateFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> ExemptControllers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Account", "Billing", "StripeWebhook"
    };

    private readonly ApplicationDbContext _db;

    public SubscriptionGateFilter(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controllerName = (context.ActionDescriptor as ControllerActionDescriptor)?.ControllerName;
        if (controllerName != null && ExemptControllers.Contains(controllerName))
        {
            await next();
            return;
        }

        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            await next();
            return;
        }

        var org = await _db.Users.Where(u => u.Id == userId).Select(u => u.Organization).FirstOrDefaultAsync();
        if (org == null)
        {
            await next();
            return;
        }

        var trialExpired = org.SubscriptionStatus == SubscriptionStatus.Trialing && org.TrialEndsAt < DateTime.UtcNow;
        var subscriptionLapsed = org.SubscriptionStatus is SubscriptionStatus.PastDue or SubscriptionStatus.Canceled;

        if (trialExpired || subscriptionLapsed)
        {
            context.Result = new RedirectToActionResult("Index", "Billing", null);
            return;
        }

        await next();
    }
}
