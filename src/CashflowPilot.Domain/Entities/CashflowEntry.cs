using CashflowPilot.Domain.Enums;

namespace CashflowPilot.Domain.Entities;

public class CashflowEntry
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int? ForecastRunId { get; set; }
    public ForecastRun? ForecastRun { get; set; }
    public int? ActualRunId { get; set; }
    public ActualRun? ActualRun { get; set; }
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;
    public int? FundId { get; set; }
    public Fund? Fund { get; set; }
    public int? StrategyId { get; set; }
    public Strategy? Strategy { get; set; }
    public DateTime Period { get; set; }
    public EntryType EntryType { get; set; }
    public decimal CapitalCalls { get; set; }
    public decimal Distributions { get; set; }
    public decimal FeeAmount { get; set; }
    public decimal ExpenseAmount { get; set; }
    public decimal NetCashflow => Distributions - CapitalCalls;
    public string Currency { get; set; } = "USD";
    public string? Notes { get; set; }
    public string? FundName { get; set; }
    public string? StrategyName { get; set; }
}
