using CashflowPilot.Application.DTOs;
using CashflowPilot.Application.Interfaces;
using CashflowPilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Infrastructure.Services;

public class ReportPackService : IReportPackService
{
    private readonly ApplicationDbContext _db;
    private readonly IVarianceService _variance;
    private readonly IScenarioService _scenario;

    public ReportPackService(ApplicationDbContext db, IVarianceService variance, IScenarioService scenario)
    {
        _db = db;
        _variance = variance;
        _scenario = scenario;
    }

    public async Task<ReportPackDto> BuildAsync(int analysisId, int organizationId)
    {
        var analysis = await _db.VarianceAnalyses
            .Include(v => v.ForecastRun)
            .Include(v => v.ActualRun)
            .Include(v => v.CommentaryPack)
            .FirstOrDefaultAsync(v => v.Id == analysisId && v.OrganizationId == organizationId)
            ?? throw new InvalidOperationException("Variance analysis not found.");

        var summary = await _variance.GetSummaryAsync(analysisId, organizationId)
            ?? throw new InvalidOperationException("Variance summary not found.");

        var topPeriods = summary.ByPeriod
            .OrderByDescending(p => Math.Abs(p.NetVariance))
            .Take(5)
            .ToList();

        var scenarioIds = await _db.Scenarios
            .Where(s => s.OrganizationId == organizationId && s.ForecastRunId == analysis.ForecastRunId)
            .Select(s => s.Id)
            .ToListAsync();
        var scenarios = new List<ScenarioResultDto>();
        foreach (var scenarioId in scenarioIds)
            scenarios.Add(await _scenario.ComputeScenarioAsync(scenarioId, organizationId));

        var uploadedFileIds = new[] { analysis.ForecastRun.UploadedFileId, analysis.ActualRun.UploadedFileId };
        var issues = await _db.ValidationIssues
            .Where(v => v.OrganizationId == organizationId && uploadedFileIds.Contains(v.UploadedFileId))
            .Include(v => v.UploadedFile)
            .OrderByDescending(v => v.Severity)
            .ThenBy(v => v.RowNumber)
            .ToListAsync();

        var commentaryPackId = analysis.CommentaryPack?.Id;
        var auditEntries = await _db.AuditLogs
            .Where(a => a.OrganizationId == organizationId &&
                ((a.EntityType == "VarianceAnalysis" && a.EntityId == analysisId.ToString()) ||
                 (commentaryPackId.HasValue && a.EntityType == "CommentaryPack" && a.EntityId == commentaryPackId.Value.ToString())))
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();

        return new ReportPackDto
        {
            Summary = summary,
            TopFundDrivers = summary.ByFund.Take(5).ToList(),
            TopPeriodDrivers = topPeriods,
            Scenarios = scenarios,
            DataQualityIssues = issues.Select(i => new DataQualityIssueDto
            {
                Severity = i.Severity.ToString(),
                FileName = i.UploadedFile?.OriginalFileName ?? "Unknown",
                RowNumber = i.RowNumber,
                FieldName = i.FieldName,
                Message = i.Message,
                IsAccepted = i.IsAccepted
            }).ToList(),
            AuditEntries = auditEntries.Select(a => new AuditEntryDto
            {
                Timestamp = a.Timestamp,
                UserName = a.UserName,
                Action = a.Action,
                Details = a.Details
            }).ToList()
        };
    }
}
