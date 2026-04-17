namespace CashflowPilot.Application.DTOs;

public class ParseResultDto
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public List<CashflowRowDto> Rows { get; set; } = new();
    public List<string> GlobalErrors { get; set; } = new();
    public List<string> DetectedHeaders { get; set; } = new();
    public ColumnMapping? Mapping { get; set; }
}

public class ColumnMapping
{
    public string? FundNameColumn { get; set; }
    public string? StrategyColumn { get; set; }
    public string? PeriodColumn { get; set; }
    public string? CapitalCallsColumn { get; set; }
    public string? DistributionsColumn { get; set; }
    public string? CurrencyColumn { get; set; }
    public string? NotesColumn { get; set; }

    public static ColumnMapping AutoDetect(IList<string> headers)
    {
        var mapping = new ColumnMapping();
        foreach (var h in headers)
        {
            var lower = h.ToLowerInvariant().Trim();
            if (lower is "fund" or "fund_name" or "fundname" or "fund name") mapping.FundNameColumn = h;
            else if (lower is "strategy" or "strategy_name" or "strategyname") mapping.StrategyColumn = h;
            else if (lower is "period" or "date" or "month" or "reporting_period") mapping.PeriodColumn = h;
            else if (lower is "capital_calls" or "capitalcalls" or "calls" or "capital calls" or "contributions") mapping.CapitalCallsColumn = h;
            else if (lower is "distributions" or "distribution" or "dist") mapping.DistributionsColumn = h;
            else if (lower is "currency" or "ccy") mapping.CurrencyColumn = h;
            else if (lower is "notes" or "comments" or "comment") mapping.NotesColumn = h;
        }
        return mapping;
    }

    public bool IsComplete() => PeriodColumn != null && (CapitalCallsColumn != null || DistributionsColumn != null);
}
