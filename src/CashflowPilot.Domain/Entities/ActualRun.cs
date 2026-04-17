using CashflowPilot.Domain.Enums;

namespace CashflowPilot.Domain.Entities;

public class ActualRun
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public DateTime ReportingPeriod { get; set; }
    public DateTime RunDate { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = string.Empty;
    public int UploadedFileId { get; set; }
    public UploadedFile UploadedFile { get; set; } = null!;
    public RunStatus Status { get; set; } = RunStatus.Pending;
    public string? ErrorMessage { get; set; }
    public int EntryCount { get; set; }
    public ICollection<CashflowEntry> CashflowEntries { get; set; } = new List<CashflowEntry>();
}
