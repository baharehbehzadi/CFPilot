using CashflowPilot.Domain.Enums;

namespace CashflowPilot.Web.Models;

public class BillingViewModel
{
    public string OrganizationName { get; set; } = string.Empty;
    public PlanTier PlanTier { get; set; }
    public SubscriptionStatus SubscriptionStatus { get; set; }
    public DateTime TrialEndsAt { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public bool HasStripeCustomer { get; set; }

    public bool IsTrialExpired => SubscriptionStatus == SubscriptionStatus.Trialing && TrialEndsAt < DateTime.UtcNow;
    public int TrialDaysRemaining => Math.Max(0, (int)Math.Ceiling((TrialEndsAt - DateTime.UtcNow).TotalDays));
}
