using CashflowPilot.Application.DTOs;
using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Entities;
using CashflowPilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Infrastructure.Services;

public class VarianceComparisonService : IVarianceComparisonService
{
    private readonly ApplicationDbContext _db;
    private readonly IScenarioService _scenarioService;

    public VarianceComparisonService(ApplicationDbContext db, IScenarioService scenarioService)
    {
        _db = db;
        _scenarioService = scenarioService;
    }

    public async Task<VarianceComparisonDto> CompareForecastToActual(int forecastRunId, int actualRunId, int organizationId)
    {
        var forecastRun = await _db.ForecastRuns.FirstOrDefaultAsync(r => r.Id == forecastRunId && r.OrganizationId == organizationId)
            ?? throw new InvalidOperationException("Forecast run not found");
        var actualRun = await _db.ActualRuns.FirstOrDefaultAsync(r => r.Id == actualRunId && r.OrganizationId == organizationId)
            ?? throw new InvalidOperationException("Actual run not found");

        var forecastEntries = await _db.CashflowEntries.Where(e => e.ForecastRunId == forecastRunId && e.OrganizationId == organizationId).ToListAsync();
        var actualEntries = await _db.CashflowEntries.Where(e => e.ActualRunId == actualRunId && e.OrganizationId == organizationId).ToListAsync();

        return BuildComparison("Forecast vs Actual", forecastRun.Name, actualRun.Name, forecastEntries, actualEntries);
    }

    public async Task<VarianceComparisonDto> CompareForecastRuns(int currentForecastRunId, int previousForecastRunId, int organizationId)
    {
        var currentRun = await _db.ForecastRuns.FirstOrDefaultAsync(r => r.Id == currentForecastRunId && r.OrganizationId == organizationId)
            ?? throw new InvalidOperationException("Forecast run not found");
        var previousRun = await _db.ForecastRuns.FirstOrDefaultAsync(r => r.Id == previousForecastRunId && r.OrganizationId == organizationId)
            ?? throw new InvalidOperationException("Forecast run not found");

        var previousEntries = await _db.CashflowEntries.Where(e => e.ForecastRunId == previousForecastRunId && e.OrganizationId == organizationId).ToListAsync();
        var currentEntries = await _db.CashflowEntries.Where(e => e.ForecastRunId == currentForecastRunId && e.OrganizationId == organizationId).ToListAsync();

        return BuildComparison("Forecast Run vs Previous Forecast Run", previousRun.Name, currentRun.Name, previousEntries, currentEntries);
    }

    public async Task<VarianceComparisonDto> CompareScenarioToBase(int scenarioId, int baseScenarioId, int organizationId)
    {
        var scenario = await _db.Scenarios.FirstOrDefaultAsync(s => s.Id == scenarioId && s.OrganizationId == organizationId)
            ?? throw new InvalidOperationException("Scenario not found");
        var baseScenario = await _db.Scenarios.FirstOrDefaultAsync(s => s.Id == baseScenarioId && s.OrganizationId == organizationId)
            ?? throw new InvalidOperationException("Scenario not found");

        var scenarioResult = await _scenarioService.ComputeScenarioAsync(scenarioId, organizationId);
        var baseResult = await _scenarioService.ComputeScenarioAsync(baseScenarioId, organizationId);

        var dto = new VarianceComparisonDto
        {
            BasisLabel = "Scenario vs Scenario",
            LeftLabel = baseScenario.Name,
            RightLabel = scenario.Name,
            LeftTotalCalls = baseResult.Periods.Sum(p => p.ScenarioCalls),
            RightTotalCalls = scenarioResult.Periods.Sum(p => p.ScenarioCalls),
            LeftTotalDistributions = baseResult.Periods.Sum(p => p.ScenarioDistributions),
            RightTotalDistributions = scenarioResult.Periods.Sum(p => p.ScenarioDistributions)
        };

        var periods = baseResult.Periods.Select(p => p.Period)
            .Union(scenarioResult.Periods.Select(p => p.Period))
            .Distinct()
            .OrderBy(p => p);

        foreach (var period in periods)
        {
            var left = baseResult.Periods.FirstOrDefault(p => p.Period == period);
            var right = scenarioResult.Periods.FirstOrDefault(p => p.Period == period);
            dto.ByPeriod.Add(new PeriodComparisonDto
            {
                Period = period,
                LeftCalls = left?.ScenarioCalls ?? 0,
                RightCalls = right?.ScenarioCalls ?? 0,
                LeftDistributions = left?.ScenarioDistributions ?? 0,
                RightDistributions = right?.ScenarioDistributions ?? 0
            });
        }

        return dto;
    }

    private static VarianceComparisonDto BuildComparison(string basisLabel, string leftLabel, string rightLabel,
        List<CashflowEntry> leftEntries, List<CashflowEntry> rightEntries)
    {
        var dto = new VarianceComparisonDto
        {
            BasisLabel = basisLabel,
            LeftLabel = leftLabel,
            RightLabel = rightLabel,
            LeftTotalCalls = leftEntries.Sum(e => e.CapitalCalls),
            RightTotalCalls = rightEntries.Sum(e => e.CapitalCalls),
            LeftTotalDistributions = leftEntries.Sum(e => e.Distributions),
            RightTotalDistributions = rightEntries.Sum(e => e.Distributions)
        };

        var periods = leftEntries.Select(e => e.Period)
            .Union(rightEntries.Select(e => e.Period))
            .Distinct()
            .OrderBy(p => p);

        foreach (var period in periods)
        {
            dto.ByPeriod.Add(new PeriodComparisonDto
            {
                Period = period,
                LeftCalls = leftEntries.Where(e => e.Period == period).Sum(e => e.CapitalCalls),
                RightCalls = rightEntries.Where(e => e.Period == period).Sum(e => e.CapitalCalls),
                LeftDistributions = leftEntries.Where(e => e.Period == period).Sum(e => e.Distributions),
                RightDistributions = rightEntries.Where(e => e.Period == period).Sum(e => e.Distributions)
            });
        }

        return dto;
    }
}
