namespace CashflowPilot.Domain.Entities;

public class NavSnapshot
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;
    public int FundId { get; set; }
    public Fund Fund { get; set; } = null!;
    public int? ReportingPeriodId { get; set; }
    public ReportingPeriod? ReportingPeriod { get; set; }
    public DateTime ValuationDate { get; set; }
    public decimal NavAmount { get; set; }
    public decimal UnfundedCommitment { get; set; }
    public decimal PaidInCapital { get; set; }
    public decimal TotalDistributions { get; set; }
    public string Currency { get; set; } = "USD";
    public int? SourceUploadedFileId { get; set; }
    public UploadedFile? SourceUploadedFile { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
