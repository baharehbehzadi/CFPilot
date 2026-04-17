namespace CashflowPilot.Domain.Entities;

public class Portfolio
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BaseCurrency { get; set; } = "USD";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public ICollection<Fund> Funds { get; set; } = new List<Fund>();
    public ICollection<ForecastRun> ForecastRuns { get; set; } = new List<ForecastRun>();
    public ICollection<ActualRun> ActualRuns { get; set; } = new List<ActualRun>();
}
