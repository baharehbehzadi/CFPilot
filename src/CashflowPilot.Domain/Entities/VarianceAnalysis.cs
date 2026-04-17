using CashflowPilot.Domain.Enums;

namespace CashflowPilot.Domain.Entities;

public class VarianceAnalysis
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int ForecastRunId { get; set; }
    public ForecastRun ForecastRun { get; set; } = null!;
    public int ActualRunId { get; set; }
    public ActualRun ActualRun { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
    public AnalysisStatus Status { get; set; } = AnalysisStatus.Pending;
    public string? Name { get; set; }
    public ICollection<VarianceEntry> Entries { get; set; } = new List<VarianceEntry>();
    public CommentaryPack? CommentaryPack { get; set; }
}

public class VarianceEntry
{
    public int Id { get; set; }
    public int VarianceAnalysisId { get; set; }
    public VarianceAnalysis VarianceAnalysis { get; set; } = null!;
    public int PortfolioId { get; set; }
    public int? FundId { get; set; }
    public string? FundName { get; set; }
    public int? StrategyId { get; set; }
    public string? StrategyName { get; set; }
    public DateTime Period { get; set; }
    public decimal ForecastCapitalCalls { get; set; }
    public decimal ActualCapitalCalls { get; set; }
    public decimal CapitalCallsVariance => ActualCapitalCalls - ForecastCapitalCalls;
    public decimal? CapitalCallsVariancePct => ForecastCapitalCalls != 0 ? (ActualCapitalCalls - ForecastCapitalCalls) / Math.Abs(ForecastCapitalCalls) * 100 : null;
    public decimal ForecastDistributions { get; set; }
    public decimal ActualDistributions { get; set; }
    public decimal DistributionsVariance => ActualDistributions - ForecastDistributions;
    public decimal? DistributionsVariancePct => ForecastDistributions != 0 ? (ActualDistributions - ForecastDistributions) / Math.Abs(ForecastDistributions) * 100 : null;
    public decimal ForecastNetCashflow => ForecastDistributions - ForecastCapitalCalls;
    public decimal ActualNetCashflow => ActualDistributions - ActualCapitalCalls;
    public decimal NetCashflowVariance => ActualNetCashflow - ForecastNetCashflow;
    public decimal? NetCashflowVariancePct => ForecastNetCashflow != 0 ? (ActualNetCashflow - ForecastNetCashflow) / Math.Abs(ForecastNetCashflow) * 100 : null;
    public decimal CumulativeVariance { get; set; }
}
