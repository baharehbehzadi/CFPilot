namespace CashflowPilot.Domain.Entities;

public class Scenario
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int ForecastRunId { get; set; }
    public ForecastRun ForecastRun { get; set; } = null!;
    public int? ReportingPeriodId { get; set; }
    public ReportingPeriod? ReportingPeriod { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal CallsAdjustmentPct { get; set; }
    public decimal DistributionsAdjustmentPct { get; set; }
    public int TimingShiftMonths { get; set; }
    public int CallsTimingShiftMonths { get; set; }
    public int DistributionsTimingShiftMonths { get; set; }
    public string Scope { get; set; } = "All";
    public int? ScopePortfolioId { get; set; }
    public int? ScopeFundId { get; set; }
    public int? ScopeStrategyId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
}
