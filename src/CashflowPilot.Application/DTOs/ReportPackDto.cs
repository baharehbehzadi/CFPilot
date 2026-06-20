namespace CashflowPilot.Application.DTOs;

public class ReportPackDto
{
    public VarianceSummaryDto Summary { get; set; } = new();
    public List<FundVarianceDto> TopFundDrivers { get; set; } = new();
    public List<PeriodVarianceDto> TopPeriodDrivers { get; set; } = new();
    public List<ScenarioResultDto> Scenarios { get; set; } = new();
    public List<DataQualityIssueDto> DataQualityIssues { get; set; } = new();
    public List<AuditEntryDto> AuditEntries { get; set; } = new();
}

public class DataQualityIssueDto
{
    public string Severity { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int? RowNumber { get; set; }
    public string? FieldName { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsAccepted { get; set; }
}

public class AuditEntryDto
{
    public DateTime Timestamp { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
}
