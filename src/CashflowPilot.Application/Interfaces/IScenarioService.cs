using CashflowPilot.Application.DTOs;

namespace CashflowPilot.Application.Interfaces;

public interface IScenarioService
{
    Task<ScenarioResultDto> ComputeScenarioAsync(int scenarioId, int organizationId);
    Task<ScenarioResultDto> PreviewScenarioAsync(int forecastRunId, ScenarioAssumptionsDto assumptions, int organizationId);
}
