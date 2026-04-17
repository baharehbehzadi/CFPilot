namespace CashflowPilot.Application.DTOs;

public class CashflowRowDto
{
    public string? FundName { get; set; }
    public string? StrategyName { get; set; }
    public string? Period { get; set; }          // raw string from CSV
    public DateTime? PeriodDate { get; set; }    // parsed
    public string? CapitalCalls { get; set; }
    public decimal? CapitalCallsAmount { get; set; }
    public string? Distributions { get; set; }
    public decimal? DistributionsAmount { get; set; }
    public string? Currency { get; set; }
    public string? Notes { get; set; }
    public int RowNumber { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public bool IsValid => ValidationErrors.Count == 0;
}
