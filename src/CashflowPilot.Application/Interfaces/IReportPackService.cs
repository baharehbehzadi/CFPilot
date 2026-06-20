using CashflowPilot.Application.DTOs;

namespace CashflowPilot.Application.Interfaces;

public interface IReportPackService
{
    Task<ReportPackDto> BuildAsync(int analysisId, int organizationId);
}
