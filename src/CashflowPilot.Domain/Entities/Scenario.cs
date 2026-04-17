namespace CashflowPilot.Domain.Entities;

public class Scenario
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int ForecastRunId { get; set; }
    public ForecastRun ForecastRun { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal CallsAdjustmentPct { get; set; }
    public decimal DistributionsAdjustmentPct { get; set; }
    public int TimingShiftMonths { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
}
