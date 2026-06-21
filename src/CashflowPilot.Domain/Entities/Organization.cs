using CashflowPilot.Domain.Enums;

namespace CashflowPilot.Domain.Entities;

public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public PlanTier PlanTier { get; set; } = PlanTier.Trial;
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Trialing;
    public DateTime TrialEndsAt { get; set; } = DateTime.UtcNow.AddDays(14);
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }

    public ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
}
