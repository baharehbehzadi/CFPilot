using CashflowPilot.Application.DTOs;
using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Entities;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Infrastructure.Services;

public class VarianceService : IVarianceService
{
    private readonly ApplicationDbContext _db;

    public VarianceService(ApplicationDbContext db) => _db = db;

    public async Task<int> CreateAnalysisAsync(int forecastRunId, int actualRunId, string userId, int organizationId, string? name = null)
    {
        var forecastEntries = await _db.CashflowEntries
            .Where(e => e.ForecastRunId == forecastRunId && e.OrganizationId == organizationId)
            .ToListAsync();

        var actualEntries = await _db.CashflowEntries
            .Where(e => e.ActualRunId == actualRunId && e.OrganizationId == organizationId)
            .ToListAsync();

        var forecastRun = await _db.ForecastRuns.FindAsync(forecastRunId);
        var actualRun = await _db.ActualRuns.FindAsync(actualRunId);

        // Group by (FundName, Period)
        var forecastByKey = forecastEntries
            .GroupBy(e => (FundName: e.FundName ?? "Unknown", Period: e.Period))
            .ToDictionary(g => g.Key, g => (Calls: g.Sum(x => x.CapitalCalls), Dists: g.Sum(x => x.Distributions), FundId: g.First().FundId, StrategyId: g.First().StrategyId, StrategyName: g.First().StrategyName));

        var actualByKey = actualEntries
            .GroupBy(e => (FundName: e.FundName ?? "Unknown", Period: e.Period))
            .ToDictionary(g => g.Key, g => (Calls: g.Sum(x => x.CapitalCalls), Dists: g.Sum(x => x.Distributions)));

        var allKeys = forecastByKey.Keys.Union(actualByKey.Keys).OrderBy(k => k.Period).ThenBy(k => k.FundName).ToList();

        var entries = new List<VarianceEntry>();
        foreach (var key in allKeys)
        {
            forecastByKey.TryGetValue(key, out var fc);
            actualByKey.TryGetValue(key, out var ac);

            entries.Add(new VarianceEntry
            {
                PortfolioId = forecastRun?.PortfolioId ?? actualRun?.PortfolioId ?? 0,
                FundId = fc.FundId,
                FundName = key.FundName,
                StrategyId = fc.StrategyId,
                StrategyName = fc.StrategyName,
                Period = key.Period,
                ForecastCapitalCalls = fc.Calls,
                ActualCapitalCalls = ac.Calls,
                ForecastDistributions = fc.Dists,
                ActualDistributions = ac.Dists,
                CumulativeVariance = 0 // set below
            });
        }

        // Compute cumulative variance (running sum of NetCashflowVariance)
        decimal cumulative = 0;
        foreach (var entry in entries.OrderBy(e => e.Period))
        {
            decimal netVariance = (entry.ActualDistributions - entry.ActualCapitalCalls) - (entry.ForecastDistributions - entry.ForecastCapitalCalls);
            cumulative += netVariance;
            entry.CumulativeVariance = cumulative;
        }

        var analysis = new VarianceAnalysis
        {
            OrganizationId = organizationId,
            ForecastRunId = forecastRunId,
            ActualRunId = actualRunId,
            CreatedByUserId = userId,
            Status = AnalysisStatus.Completed,
            Name = name ?? $"Analysis {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
            CreatedAt = DateTime.UtcNow,
            Entries = entries
        };

        _db.VarianceAnalyses.Add(analysis);
        await _db.SaveChangesAsync();
        return analysis.Id;
    }

    public async Task<VarianceSummaryDto?> GetSummaryAsync(int analysisId, int organizationId)
    {
        var analysis = await _db.VarianceAnalyses
            .Include(v => v.Entries)
            .Include(v => v.ForecastRun)
            .Include(v => v.ActualRun)
            .FirstOrDefaultAsync(v => v.Id == analysisId && v.OrganizationId == organizationId);

        if (analysis == null) return null;

        var entries = analysis.Entries.ToList();

        var dto = new VarianceSummaryDto
        {
            AnalysisId = analysis.Id,
            AnalysisName = analysis.Name ?? string.Empty,
            ForecastRunName = analysis.ForecastRun.Name,
            ActualRunName = analysis.ActualRun.Name,
            CreatedAt = analysis.CreatedAt,
            TotalForecastCalls = entries.Sum(e => e.ForecastCapitalCalls),
            TotalActualCalls = entries.Sum(e => e.ActualCapitalCalls),
            TotalForecastDistributions = entries.Sum(e => e.ForecastDistributions),
            TotalActualDistributions = entries.Sum(e => e.ActualDistributions)
        };

        dto.TotalCallsVariance = dto.TotalActualCalls - dto.TotalForecastCalls;
        dto.TotalCallsVariancePct = dto.TotalForecastCalls != 0 ? dto.TotalCallsVariance / Math.Abs(dto.TotalForecastCalls) * 100 : null;
        dto.TotalDistributionsVariance = dto.TotalActualDistributions - dto.TotalForecastDistributions;
        dto.TotalDistributionsVariancePct = dto.TotalForecastDistributions != 0 ? dto.TotalDistributionsVariance / Math.Abs(dto.TotalForecastDistributions) * 100 : null;
        dto.TotalForecastNetCashflow = dto.TotalForecastDistributions - dto.TotalForecastCalls;
        dto.TotalActualNetCashflow = dto.TotalActualDistributions - dto.TotalActualCalls;
        dto.TotalNetCashflowVariance = dto.TotalActualNetCashflow - dto.TotalForecastNetCashflow;
        dto.TotalNetCashflowVariancePct = dto.TotalForecastNetCashflow != 0 ? dto.TotalNetCashflowVariance / Math.Abs(dto.TotalForecastNetCashflow) * 100 : null;

        // By period
        decimal cumPeriod = 0;
        dto.ByPeriod = entries
            .GroupBy(e => e.Period)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var fCalls = g.Sum(e => e.ForecastCapitalCalls);
                var aCalls = g.Sum(e => e.ActualCapitalCalls);
                var fDists = g.Sum(e => e.ForecastDistributions);
                var aDists = g.Sum(e => e.ActualDistributions);
                var fNet = fDists - fCalls;
                var aNet = aDists - aCalls;
                var variance = aNet - fNet;
                cumPeriod += variance;
                return new PeriodVarianceDto
                {
                    Period = g.Key,
                    ForecastCalls = fCalls, ActualCalls = aCalls, CallsVariance = aCalls - fCalls,
                    ForecastDistributions = fDists, ActualDistributions = aDists, DistributionsVariance = aDists - fDists,
                    ForecastNet = fNet, ActualNet = aNet, NetVariance = variance,
                    CumulativeVariance = cumPeriod
                };
            }).ToList();

        // By fund
        dto.ByFund = entries
            .GroupBy(e => e.FundName ?? "Unknown")
            .Select(g => new FundVarianceDto
            {
                FundName = g.Key,
                TotalCallsVariance = g.Sum(e => e.ActualCapitalCalls - e.ForecastCapitalCalls),
                TotalDistributionsVariance = g.Sum(e => e.ActualDistributions - e.ForecastDistributions),
                TotalNetVariance = g.Sum(e => (e.ActualDistributions - e.ActualCapitalCalls) - (e.ForecastDistributions - e.ForecastCapitalCalls))
            })
            .OrderByDescending(f => Math.Abs(f.TotalNetVariance))
            .ToList();

        return dto;
    }
}
