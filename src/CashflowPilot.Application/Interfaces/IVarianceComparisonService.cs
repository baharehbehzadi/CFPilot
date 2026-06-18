using CashflowPilot.Application.DTOs;

namespace CashflowPilot.Application.Interfaces;

public interface IVarianceComparisonService
{
    Task<VarianceComparisonDto> CompareForecastToActual(int forecastRunId, int actualRunId, int organizationId);
    Task<VarianceComparisonDto> CompareForecastRuns(int currentForecastRunId, int previousForecastRunId, int organizationId);
    Task<VarianceComparisonDto> CompareScenarioToBase(int scenarioId, int baseScenarioId, int organizationId);
}
