using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Entities;
using CashflowPilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Infrastructure.Services;

public class CommentaryService : ICommentaryService
{
    private readonly ApplicationDbContext _db;

    public CommentaryService(ApplicationDbContext db) => _db = db;

    public async Task<CommentaryPack> GenerateAsync(int analysisId, string userId, int organizationId)
    {
        var analysis = await _db.VarianceAnalyses
            .Include(v => v.Entries)
            .Include(v => v.ForecastRun)
            .Include(v => v.ActualRun)
            .FirstOrDefaultAsync(v => v.Id == analysisId && v.OrganizationId == organizationId)
            ?? throw new InvalidOperationException($"Variance analysis {analysisId} not found.");

        var entries = analysis.Entries.ToList();

        decimal totalForecastNet = entries.Sum(e => e.ForecastDistributions - e.ForecastCapitalCalls);
        decimal totalActualNet = entries.Sum(e => e.ActualDistributions - e.ActualCapitalCalls);
        decimal totalNetVariance = totalActualNet - totalForecastNet;
        string direction = totalNetVariance >= 0 ? "positive" : "negative";
        string absVar = FormatM(Math.Abs(totalNetVariance));

        // Executive summary
        var execSummary = $"For the period covered by {analysis.ActualRun.Name}, the portfolio reported a net cashflow of " +
            $"{FormatM(totalActualNet)} against a forecast of {FormatM(totalForecastNet)}, " +
            $"representing a {direction} net cashflow variance of {absVar} " +
            $"({FormatPct(totalForecastNet != 0 ? (totalNetVariance / Math.Abs(totalForecastNet) * 100) : 0)}). " +
            $"A total of {entries.Count} fund-period combinations were analysed across this report.";

        // Positive variances (actual net > forecast net)
        var positive = entries
            .Where(e => (e.ActualDistributions - e.ActualCapitalCalls) - (e.ForecastDistributions - e.ForecastCapitalCalls) > 0)
            .OrderByDescending(e => (e.ActualDistributions - e.ActualCapitalCalls) - (e.ForecastDistributions - e.ForecastCapitalCalls))
            .Take(5)
            .ToList();
        var posText = positive.Any()
            ? "Key positive variances: " + string.Join("; ", positive.Select(e =>
                $"{e.FundName ?? "Unknown"} ({e.Period:MMM-yyyy}): actual net {FormatM(e.ActualDistributions - e.ActualCapitalCalls)} vs forecast {FormatM(e.ForecastDistributions - e.ForecastCapitalCalls)} (variance +{FormatM((e.ActualDistributions - e.ActualCapitalCalls) - (e.ForecastDistributions - e.ForecastCapitalCalls))})"))
            : "No material positive variances recorded in this period.";

        // Negative variances
        var negative = entries
            .Where(e => (e.ActualDistributions - e.ActualCapitalCalls) - (e.ForecastDistributions - e.ForecastCapitalCalls) < 0)
            .OrderBy(e => (e.ActualDistributions - e.ActualCapitalCalls) - (e.ForecastDistributions - e.ForecastCapitalCalls))
            .Take(5)
            .ToList();
        var negText = negative.Any()
            ? "Key negative variances: " + string.Join("; ", negative.Select(e =>
                $"{e.FundName ?? "Unknown"} ({e.Period:MMM-yyyy}): actual net {FormatM(e.ActualDistributions - e.ActualCapitalCalls)} vs forecast {FormatM(e.ForecastDistributions - e.ForecastCapitalCalls)} (variance {FormatM((e.ActualDistributions - e.ActualCapitalCalls) - (e.ForecastDistributions - e.ForecastCapitalCalls))})"))
            : "No material negative variances recorded in this period.";

        // Delayed distributions: actual distributions < 50% of forecast distributions
        var delayed = entries
            .Where(e => e.ForecastDistributions > 0 && e.ActualDistributions < e.ForecastDistributions * 0.5m)
            .OrderByDescending(e => e.ForecastDistributions - e.ActualDistributions)
            .Take(5)
            .ToList();
        var delayedText = delayed.Any()
            ? "Funds with delayed or lower-than-expected distributions: " + string.Join("; ", delayed.Select(e =>
                $"{e.FundName ?? "Unknown"} ({e.Period:MMM-yyyy}): forecast {FormatM(e.ForecastDistributions)}, actual {FormatM(e.ActualDistributions)} (shortfall {FormatM(e.ForecastDistributions - e.ActualDistributions)})"))
            : "No material distribution delays identified.";

        // Higher-than-expected calls: actual calls > 120% of forecast
        var higherCalls = entries
            .Where(e => e.ForecastCapitalCalls > 0 && e.ActualCapitalCalls > e.ForecastCapitalCalls * 1.2m)
            .OrderByDescending(e => e.ActualCapitalCalls - e.ForecastCapitalCalls)
            .Take(5)
            .ToList();
        var callsText = higherCalls.Any()
            ? "Funds with higher-than-expected capital calls: " + string.Join("; ", higherCalls.Select(e =>
                $"{e.FundName ?? "Unknown"} ({e.Period:MMM-yyyy}): forecast {FormatM(e.ForecastCapitalCalls)}, actual {FormatM(e.ActualCapitalCalls)} (excess {FormatM(e.ActualCapitalCalls - e.ForecastCapitalCalls)})"))
            : "No material excess capital calls identified.";

        // Watchpoints: cumulative variance > 5% of total forecast net OR largest single period variance
        var bigVariances = entries
            .Where(e => e.ForecastCapitalCalls + e.ForecastDistributions > 0)
            .Select(e => new { e.FundName, e.Period, NetVar = (e.ActualDistributions - e.ActualCapitalCalls) - (e.ForecastDistributions - e.ForecastCapitalCalls) })
            .Where(x => Math.Abs(x.NetVar) > Math.Abs(totalForecastNet) * 0.05m && totalForecastNet != 0)
            .OrderByDescending(x => Math.Abs(x.NetVar))
            .Take(3)
            .ToList();
        var watchText = bigVariances.Any()
            ? "Watchpoints — items with variance exceeding 5% of total forecast net cashflow: " +
              string.Join("; ", bigVariances.Select(w => $"{w.FundName ?? "Unknown"} ({w.Period:MMM-yyyy}): net variance {FormatM(w.NetVar)}"))
            : "No individual fund-period variances exceeded the 5% materiality threshold.";

        var pack = new CommentaryPack
        {
            OrganizationId = organizationId,
            VarianceAnalysisId = analysisId,
            GeneratedAt = DateTime.UtcNow,
            GeneratedByUserId = userId,
            ExecutiveSummary = execSummary,
            PositiveVariances = posText,
            NegativeVariances = negText,
            DelayedDistributions = delayedText,
            HigherThanExpectedCalls = callsText,
            Watchpoints = watchText
        };

        // Remove existing commentary if re-generating
        var existing = await _db.CommentaryPacks.FirstOrDefaultAsync(c => c.VarianceAnalysisId == analysisId);
        if (existing != null) _db.CommentaryPacks.Remove(existing);

        _db.CommentaryPacks.Add(pack);
        await _db.SaveChangesAsync();
        return pack;
    }

    private static string FormatM(decimal value) => $"${value:N0}";
    private static string FormatPct(decimal value) => $"{value:+0.##;-0.##;0}%";
}
