using CashflowPilot.Application.DTOs;
using CashflowPilot.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace CashflowPilot.Web.Models;

public class CreateScenarioViewModel
{
    [Required, StringLength(200)] public string Name { get; set; } = string.Empty;
    [StringLength(500)] public string? Description { get; set; }
    [Required] public int ForecastRunId { get; set; }
    [Range(-100, 500)] public decimal CallsAdjustmentPct { get; set; } = 0;
    [Range(-100, 500)] public decimal DistributionsAdjustmentPct { get; set; } = 0;
    [Range(-24, 24)] public int TimingShiftMonths { get; set; } = 0;
    public List<ForecastRun> ForecastRuns { get; set; } = new();
}

public class ScenarioDetailViewModel
{
    public Scenario Scenario { get; set; } = null!;
    public ScenarioResultDto Result { get; set; } = null!;
}

public class ScenarioListViewModel
{
    public List<Scenario> Scenarios { get; set; } = new();
}
