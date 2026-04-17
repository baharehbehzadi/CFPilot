namespace CashflowPilot.Domain.Entities;

public class Strategy
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int FundId { get; set; }
    public Fund Fund { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
