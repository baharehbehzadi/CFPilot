namespace CashflowPilot.Domain.Entities;

public class OrganizationSettings
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public decimal MaterialVarianceAmount { get; set; } = 250_000m;
    public decimal MaterialVariancePct { get; set; } = 10m;
    public int TopDriverCount { get; set; } = 5;
    public decimal WatchpointThresholdPct { get; set; } = 15m;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
