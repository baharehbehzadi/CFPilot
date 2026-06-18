namespace CashflowPilot.Application.DTOs;

public class NavSnapshotCreateDto
{
    public int PortfolioId { get; set; }
    public int FundId { get; set; }
    public int? ReportingPeriodId { get; set; }
    public DateTime ValuationDate { get; set; }
    public decimal NavAmount { get; set; }
    public decimal UnfundedCommitment { get; set; }
    public decimal PaidInCapital { get; set; }
    public decimal TotalDistributions { get; set; }
    public string Currency { get; set; } = "USD";
}
