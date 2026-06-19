namespace CashflowPilot.Application.DTOs;

public class ScenarioAssumptionsDto
{
    public decimal CallsAdjustmentPct { get; set; }
    public decimal DistributionsAdjustmentPct { get; set; }
    public int CallsTimingShiftMonths { get; set; }
    public int DistributionsTimingShiftMonths { get; set; }
    public string Scope { get; set; } = "All";
    public int? ScopePortfolioId { get; set; }
    public int? ScopeFundId { get; set; }
    public int? ScopeStrategyId { get; set; }
}
