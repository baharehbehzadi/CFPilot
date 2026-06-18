using CashflowPilot.Domain.Enums;

namespace CashflowPilot.Application.DTOs;

public class RowValidationIssueDto
{
    public ValidationSeverity Severity { get; set; }
    public string? FieldName { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? SuggestedFix { get; set; }
}

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
    public List<RowValidationIssueDto> Issues { get; set; } = new();
    public List<string> ValidationErrors => Issues.Where(i => i.Severity == ValidationSeverity.Error).Select(i => i.Message).ToList();
    public bool IsValid => !Issues.Any(i => i.Severity == ValidationSeverity.Error);
}
