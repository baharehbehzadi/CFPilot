namespace CashflowPilot.Application.DTOs;

public class CommentaryContentDto
{
    public string ExecutiveSummary { get; set; } = string.Empty;
    public string PositiveVariances { get; set; } = string.Empty;
    public string NegativeVariances { get; set; } = string.Empty;
    public string DelayedDistributions { get; set; } = string.Empty;
    public string HigherThanExpectedCalls { get; set; } = string.Empty;
    public string Watchpoints { get; set; } = string.Empty;
}
