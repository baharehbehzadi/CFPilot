using CashflowPilot.Application.DTOs;
using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Entities;
using CashflowPilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Infrastructure.Services;

public class ScenarioService : IScenarioService
{
    private readonly ApplicationDbContext _db;

    public ScenarioService(ApplicationDbContext db) => _db = db;

    public async Task<ScenarioResultDto> ComputeScenarioAsync(int scenarioId, int organizationId)
    {
        var scenario = await _db.Scenarios
            .FirstOrDefaultAsync(s => s.Id == scenarioId && s.OrganizationId == organizationId)
            ?? throw new InvalidOperationException($"Scenario {scenarioId} not found.");

        var entries = await _db.CashflowEntries
            .Where(e => e.ForecastRunId == scenario.ForecastRunId && e.OrganizationId == organizationId)
            .ToListAsync();

        return BuildResult(scenario.Id, scenario.Name, scenario.Description ?? string.Empty,
            scenario.CallsAdjustmentPct, scenario.DistributionsAdjustmentPct, scenario.TimingShiftMonths, entries);
    }

    public async Task<ScenarioResultDto> PreviewScenarioAsync(int forecastRunId, decimal callsAdjPct, decimal distAdjPct, int timingShiftMonths, int organizationId)
    {
        var entries = await _db.CashflowEntries
            .Where(e => e.ForecastRunId == forecastRunId && e.OrganizationId == organizationId)
            .ToListAsync();

        return BuildResult(0, "Preview", "Ad-hoc scenario preview", callsAdjPct, distAdjPct, timingShiftMonths, entries);
    }

    private static ScenarioResultDto BuildResult(int scenarioId, string name, string description,
        decimal callsAdjPct, decimal distAdjPct, int timingShiftMonths, List<CashflowEntry> entries)
    {
        var grouped = entries
            .GroupBy(e => e.Period)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Period = g.Key,
                BaselineCalls = g.Sum(e => e.CapitalCalls),
                BaselineDists = g.Sum(e => e.Distributions)
            }).ToList();

        // Apply pct adjustment to baseline, then shift timing
        var adjusted = grouped.Select(g => new
        {
            OriginalPeriod = g.Period,
            ShiftedPeriod = g.Period.AddMonths(timingShiftMonths),
            ScenarioCalls = g.BaselineCalls * (1 + callsAdjPct / 100m),
            ScenarioDists = g.BaselineDists * (1 + distAdjPct / 100m),
            g.BaselineCalls,
            g.BaselineDists
        }).ToList();

        // Merge baseline periods and scenario periods (scenario may have shifted to new periods)
        var allPeriods = grouped.Select(g => g.Period)
            .Union(adjusted.Select(a => a.ShiftedPeriod))
            .OrderBy(p => p)
            .ToList();

        var baselineByPeriod = grouped.ToDictionary(g => g.Period, g => (g.BaselineCalls, g.BaselineDists));
        var scenarioByPeriod = adjusted
            .GroupBy(a => a.ShiftedPeriod)
            .ToDictionary(g => g.Key, g => (Calls: g.Sum(a => a.ScenarioCalls), Dists: g.Sum(a => a.ScenarioDists)));

        var periods = allPeriods.Select(p =>
        {
            baselineByPeriod.TryGetValue(p, out var b);
            scenarioByPeriod.TryGetValue(p, out var s);
            var baseNet = b.BaselineDists - b.BaselineCalls;
            var scenNet = s.Dists - s.Calls;
            return new ScenarioPeriodDto
            {
                Period = p,
                BaselineCalls = b.BaselineCalls,
                ScenarioCalls = s.Calls,
                BaselineDistributions = b.BaselineDists,
                ScenarioDistributions = s.Dists,
                BaselineNet = baseNet,
                ScenarioNet = scenNet,
                Delta = scenNet - baseNet
            };
        }).ToList();

        var baseTotal = periods.Sum(p => p.BaselineNet);
        var scenTotal = periods.Sum(p => p.ScenarioNet);

        return new ScenarioResultDto
        {
            ScenarioId = scenarioId,
            ScenarioName = name,
            Description = description,
            CallsAdjustmentPct = callsAdjPct,
            DistributionsAdjustmentPct = distAdjPct,
            TimingShiftMonths = timingShiftMonths,
            Periods = periods,
            BaselineNetCashflow = baseTotal,
            ScenarioNetCashflow = scenTotal,
            NetCashflowDelta = scenTotal - baseTotal
        };
    }
}
