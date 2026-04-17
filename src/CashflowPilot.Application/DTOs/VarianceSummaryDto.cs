namespace CashflowPilot.Application.DTOs;

public class VarianceSummaryDto
{
    public int AnalysisId { get; set; }
    public string AnalysisName { get; set; } = string.Empty;
    public string ForecastRunName { get; set; } = string.Empty;
    public string ActualRunName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public decimal TotalForecastCalls { get; set; }
    public decimal TotalActualCalls { get; set; }
    public decimal TotalCallsVariance { get; set; }
    public decimal? TotalCallsVariancePct { get; set; }

    public decimal TotalForecastDistributions { get; set; }
    public decimal TotalActualDistributions { get; set; }
    public decimal TotalDistributionsVariance { get; set; }
    public decimal? TotalDistributionsVariancePct { get; set; }

    public decimal TotalForecastNetCashflow { get; set; }
    public decimal TotalActualNetCashflow { get; set; }
    public decimal TotalNetCashflowVariance { get; set; }
    public decimal? TotalNetCashflowVariancePct { get; set; }

    public List<PeriodVarianceDto> ByPeriod { get; set; } = new();
    public List<FundVarianceDto> ByFund { get; set; } = new();
}

public class PeriodVarianceDto
{
    public DateTime Period { get; set; }
    public decimal ForecastCalls { get; set; }
    public decimal ActualCalls { get; set; }
    public decimal CallsVariance { get; set; }
    public decimal ForecastDistributions { get; set; }
    public decimal ActualDistributions { get; set; }
    public decimal DistributionsVariance { get; set; }
    public decimal ForecastNet { get; set; }
    public decimal ActualNet { get; set; }
    public decimal NetVariance { get; set; }
    public decimal CumulativeVariance { get; set; }
}

public class FundVarianceDto
{
    public string FundName { get; set; } = string.Empty;
    public decimal TotalCallsVariance { get; set; }
    public decimal TotalDistributionsVariance { get; set; }
    public decimal TotalNetVariance { get; set; }
}
