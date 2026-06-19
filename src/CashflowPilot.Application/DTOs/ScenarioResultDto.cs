namespace CashflowPilot.Application.DTOs;

public class ScenarioResultDto
{
    public int ScenarioId { get; set; }
    public string ScenarioName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal CallsAdjustmentPct { get; set; }
    public decimal DistributionsAdjustmentPct { get; set; }
    public int CallsTimingShiftMonths { get; set; }
    public int DistributionsTimingShiftMonths { get; set; }
    public List<ScenarioPeriodDto> Periods { get; set; } = new();
    public decimal BaselineNetCashflow { get; set; }
    public decimal ScenarioNetCashflow { get; set; }
    public decimal NetCashflowDelta { get; set; }
}

public class ScenarioPeriodDto
{
    public DateTime Period { get; set; }
    public decimal BaselineCalls { get; set; }
    public decimal ScenarioCalls { get; set; }
    public decimal BaselineDistributions { get; set; }
    public decimal ScenarioDistributions { get; set; }
    public decimal BaselineNet { get; set; }
    public decimal ScenarioNet { get; set; }
    public decimal Delta { get; set; }
}
