using CashflowPilot.Application.DTOs;

namespace CashflowPilot.Application.Interfaces;

public interface IScenarioService
{
    Task<ScenarioResultDto> ComputeScenarioAsync(int scenarioId, int organizationId);
    Task<ScenarioResultDto> PreviewScenarioAsync(int forecastRunId, decimal callsAdjPct, decimal distAdjPct, int timingShiftMonths, int organizationId);
}
