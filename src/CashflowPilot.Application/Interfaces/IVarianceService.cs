using CashflowPilot.Application.DTOs;

namespace CashflowPilot.Application.Interfaces;

public interface IVarianceService
{
    Task<int> CreateAnalysisAsync(int forecastRunId, int actualRunId, string userId, int organizationId, string? name = null);
    Task<VarianceSummaryDto?> GetSummaryAsync(int analysisId, int organizationId);
}
