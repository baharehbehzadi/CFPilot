using CashflowPilot.Domain.Enums;

namespace CashflowPilot.Domain.Entities;

public class ReportingPeriod
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime PeriodStartDate { get; set; }
    public DateTime PeriodEndDate { get; set; }
    public PeriodFrequency Frequency { get; set; } = PeriodFrequency.Quarterly;
    public bool IsClosed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
