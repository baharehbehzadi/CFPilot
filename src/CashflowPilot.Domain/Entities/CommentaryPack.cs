namespace CashflowPilot.Domain.Entities;

public class CommentaryPack
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int VarianceAnalysisId { get; set; }
    public VarianceAnalysis VarianceAnalysis { get; set; } = null!;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string GeneratedByUserId { get; set; } = string.Empty;
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string PositiveVariances { get; set; } = string.Empty;
    public string NegativeVariances { get; set; } = string.Empty;
    public string DelayedDistributions { get; set; } = string.Empty;
    public string HigherThanExpectedCalls { get; set; } = string.Empty;
    public string Watchpoints { get; set; } = string.Empty;
    public string? AdditionalNotes { get; set; }
}
