using CashflowPilot.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace CashflowPilot.Web.Models;

public class CreateNavSnapshotViewModel
{
    [Required] public int PortfolioId { get; set; }
    [Required] public int FundId { get; set; }
    public int? ReportingPeriodId { get; set; }
    [Required] public DateTime ValuationDate { get; set; } = DateTime.UtcNow.Date;
    [Range(0, double.MaxValue)] public decimal NavAmount { get; set; }
    [Range(0, double.MaxValue)] public decimal UnfundedCommitment { get; set; }
    [Range(0, double.MaxValue)] public decimal PaidInCapital { get; set; }
    [Range(0, double.MaxValue)] public decimal TotalDistributions { get; set; }
    [Required, StringLength(10)] public string Currency { get; set; } = "USD";
    public List<Portfolio> Portfolios { get; set; } = new();
    public List<Fund> Funds { get; set; } = new();
    public List<ReportingPeriod> ReportingPeriods { get; set; } = new();
}

public class NavSnapshotListViewModel
{
    public List<NavSnapshot> Snapshots { get; set; } = new();
}
