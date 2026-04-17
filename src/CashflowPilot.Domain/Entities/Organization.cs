namespace CashflowPilot.Domain.Entities;

public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
}
