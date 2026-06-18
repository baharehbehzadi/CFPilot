namespace CashflowPilot.Application.DTOs;

public class VarianceComparisonDto
{
    public string BasisLabel { get; set; } = string.Empty;
    public string LeftLabel { get; set; } = string.Empty;
    public string RightLabel { get; set; } = string.Empty;
    public decimal LeftTotalCalls { get; set; }
    public decimal RightTotalCalls { get; set; }
    public decimal CallsDelta => RightTotalCalls - LeftTotalCalls;
    public decimal? CallsDeltaPct => LeftTotalCalls != 0 ? CallsDelta / Math.Abs(LeftTotalCalls) * 100 : null;
    public decimal LeftTotalDistributions { get; set; }
    public decimal RightTotalDistributions { get; set; }
    public decimal DistributionsDelta => RightTotalDistributions - LeftTotalDistributions;
    public decimal? DistributionsDeltaPct => LeftTotalDistributions != 0 ? DistributionsDelta / Math.Abs(LeftTotalDistributions) * 100 : null;
    public decimal LeftNetCashflow => LeftTotalDistributions - LeftTotalCalls;
    public decimal RightNetCashflow => RightTotalDistributions - RightTotalCalls;
    public decimal NetCashflowDelta => RightNetCashflow - LeftNetCashflow;
    public decimal? NetCashflowDeltaPct => LeftNetCashflow != 0 ? NetCashflowDelta / Math.Abs(LeftNetCashflow) * 100 : null;
    public List<PeriodComparisonDto> ByPeriod { get; set; } = new();
}

public class PeriodComparisonDto
{
    public DateTime Period { get; set; }
    public decimal LeftCalls { get; set; }
    public decimal RightCalls { get; set; }
    public decimal LeftDistributions { get; set; }
    public decimal RightDistributions { get; set; }
    public decimal LeftNet => LeftDistributions - LeftCalls;
    public decimal RightNet => RightDistributions - RightCalls;
    public decimal NetDelta => RightNet - LeftNet;
}
