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

        var assumptions = new ScenarioAssumptionsDto
        {
            CallsAdjustmentPct = scenario.CallsAdjustmentPct,
            DistributionsAdjustmentPct = scenario.DistributionsAdjustmentPct,
            CallsTimingShiftMonths = scenario.CallsTimingShiftMonths,
            DistributionsTimingShiftMonths = scenario.DistributionsTimingShiftMonths,
            Scope = scenario.Scope,
            ScopePortfolioId = scenario.ScopePortfolioId,
            ScopeFundId = scenario.ScopeFundId,
            ScopeStrategyId = scenario.ScopeStrategyId
        };

        return BuildResult(scenario.Id, scenario.Name, scenario.Description ?? string.Empty, assumptions, entries);
    }

    public async Task<ScenarioResultDto> PreviewScenarioAsync(int forecastRunId, ScenarioAssumptionsDto assumptions, int organizationId)
    {
        var entries = await _db.CashflowEntries
            .Where(e => e.ForecastRunId == forecastRunId && e.OrganizationId == organizationId)
            .ToListAsync();

        return BuildResult(0, "Preview", "Ad-hoc scenario preview", assumptions, entries);
    }

    private static bool IsInScope(CashflowEntry entry, ScenarioAssumptionsDto a) => a.Scope switch
    {
        "Portfolio" => entry.PortfolioId == a.ScopePortfolioId,
        "Fund" => entry.FundId == a.ScopeFundId,
        "Strategy" => entry.StrategyId == a.ScopeStrategyId,
        _ => true
    };

    private static ScenarioResultDto BuildResult(int scenarioId, string name, string description,
        ScenarioAssumptionsDto a, List<CashflowEntry> entries)
    {
        decimal callsFactor = 1 + a.CallsAdjustmentPct / 100m;
        decimal distFactor = 1 + a.DistributionsAdjustmentPct / 100m;

        var baselineCallsByPeriod = new Dictionary<DateTime, decimal>();
        var baselineDistByPeriod = new Dictionary<DateTime, decimal>();
        var scenarioCallsByPeriod = new Dictionary<DateTime, decimal>();
        var scenarioDistByPeriod = new Dictionary<DateTime, decimal>();

        static void Add(Dictionary<DateTime, decimal> dict, DateTime period, decimal amount) =>
            dict[period] = dict.TryGetValue(period, out var existing) ? existing + amount : amount;

        // Scoped entries (matching the scenario's fund/strategy/portfolio filter) get the
        // pct adjustment and timing shift applied; out-of-scope entries pass through unchanged
        // so the rest of the portfolio stays at baseline in the scenario line.
        foreach (var entry in entries)
        {
            Add(baselineCallsByPeriod, entry.Period, entry.CapitalCalls);
            Add(baselineDistByPeriod, entry.Period, entry.Distributions);

            bool inScope = IsInScope(entry, a);
            var callsAmount = inScope ? entry.CapitalCalls * callsFactor : entry.CapitalCalls;
            var distAmount = inScope ? entry.Distributions * distFactor : entry.Distributions;
            var callsPeriod = inScope ? entry.Period.AddMonths(a.CallsTimingShiftMonths) : entry.Period;
            var distPeriod = inScope ? entry.Period.AddMonths(a.DistributionsTimingShiftMonths) : entry.Period;

            Add(scenarioCallsByPeriod, callsPeriod, callsAmount);
            Add(scenarioDistByPeriod, distPeriod, distAmount);
        }

        var allPeriods = baselineCallsByPeriod.Keys
            .Union(baselineDistByPeriod.Keys)
            .Union(scenarioCallsByPeriod.Keys)
            .Union(scenarioDistByPeriod.Keys)
            .OrderBy(p => p)
            .ToList();

        var periods = allPeriods.Select(p =>
        {
            var baseCalls = baselineCallsByPeriod.GetValueOrDefault(p);
            var baseDist = baselineDistByPeriod.GetValueOrDefault(p);
            var scenCalls = scenarioCallsByPeriod.GetValueOrDefault(p);
            var scenDist = scenarioDistByPeriod.GetValueOrDefault(p);
            var baseNet = baseDist - baseCalls;
            var scenNet = scenDist - scenCalls;
            return new ScenarioPeriodDto
            {
                Period = p,
                BaselineCalls = baseCalls,
                ScenarioCalls = scenCalls,
                BaselineDistributions = baseDist,
                ScenarioDistributions = scenDist,
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
            CallsAdjustmentPct = a.CallsAdjustmentPct,
            DistributionsAdjustmentPct = a.DistributionsAdjustmentPct,
            CallsTimingShiftMonths = a.CallsTimingShiftMonths,
            DistributionsTimingShiftMonths = a.DistributionsTimingShiftMonths,
            Periods = periods,
            BaselineNetCashflow = baseTotal,
            ScenarioNetCashflow = scenTotal,
            NetCashflowDelta = scenTotal - baseTotal
        };
    }
}
