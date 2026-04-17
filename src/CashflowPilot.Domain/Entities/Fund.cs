namespace CashflowPilot.Domain.Entities;

public class Fund
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public int PortfolioId { get; set; }
    public Portfolio Portfolio { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public int? Vintage { get; set; }
    public string? AssetClass { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public ICollection<Strategy> Strategies { get; set; } = new List<Strategy>();
    public ICollection<CashflowEntry> CashflowEntries { get; set; } = new List<CashflowEntry>();
}
