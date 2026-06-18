using CashflowPilot.Application.DTOs;
using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Entities;

namespace CashflowPilot.Infrastructure.Services;

public class RuleBasedCommentaryGenerator : ICommentaryGenerator
{
    public CommentaryContentDto Generate(VarianceAnalysis analysis, List<VarianceEntry> entries, OrganizationSettings settings)
    {
        decimal totalForecastNet = entries.Sum(e => e.ForecastNetCashflow);
        decimal totalActualNet = entries.Sum(e => e.ActualNetCashflow);
        decimal totalNetVariance = totalActualNet - totalForecastNet;
        string direction = totalNetVariance >= 0 ? "positive" : "negative";
        string absVar = FormatM(Math.Abs(totalNetVariance));

        bool IsMaterial(decimal netVar) =>
            Math.Abs(netVar) >= settings.MaterialVarianceAmount ||
            (totalForecastNet != 0 && Math.Abs(netVar) / Math.Abs(totalForecastNet) * 100 >= settings.MaterialVariancePct);

        // Executive summary
        var execSummary = $"For the period covered by {analysis.ActualRun.Name}, the portfolio reported a net cashflow of " +
            $"{FormatM(totalActualNet)} against a forecast of {FormatM(totalForecastNet)}, " +
            $"representing a {direction} net cashflow variance of {absVar} " +
            $"({FormatPct(totalForecastNet != 0 ? (totalNetVariance / Math.Abs(totalForecastNet) * 100) : 0)}). " +
            $"A total of {entries.Count} fund-period combinations were analysed across this report.";

        // Positive variances (actual net > forecast net), filtered to material items
        var positive = entries
            .Where(e => e.NetCashflowVariance > 0 && IsMaterial(e.NetCashflowVariance))
            .OrderByDescending(e => e.NetCashflowVariance)
            .Take(settings.TopDriverCount)
            .ToList();
        var posText = positive.Any()
            ? "Key positive variances: " + string.Join("; ", positive.Select(e =>
                $"{e.FundName ?? "Unknown"} ({e.Period:MMM-yyyy}): actual net {FormatM(e.ActualNetCashflow)} vs forecast {FormatM(e.ForecastNetCashflow)} (variance +{FormatM(e.NetCashflowVariance)})"))
            : "No material positive variances recorded in this period.";

        // Negative variances, filtered to material items
        var negative = entries
            .Where(e => e.NetCashflowVariance < 0 && IsMaterial(e.NetCashflowVariance))
            .OrderBy(e => e.NetCashflowVariance)
            .Take(settings.TopDriverCount)
            .ToList();
        var negText = negative.Any()
            ? "Key negative variances: " + string.Join("; ", negative.Select(e =>
                $"{e.FundName ?? "Unknown"} ({e.Period:MMM-yyyy}): actual net {FormatM(e.ActualNetCashflow)} vs forecast {FormatM(e.ForecastNetCashflow)} (variance {FormatM(e.NetCashflowVariance)})"))
            : "No material negative variances recorded in this period.";

        // Delayed distributions: actual distributions < 50% of forecast distributions
        var delayed = entries
            .Where(e => e.ForecastDistributions > 0 && e.ActualDistributions < e.ForecastDistributions * 0.5m)
            .OrderByDescending(e => e.ForecastDistributions - e.ActualDistributions)
            .Take(settings.TopDriverCount)
            .ToList();
        var delayedText = delayed.Any()
            ? "Funds with delayed or lower-than-expected distributions: " + string.Join("; ", delayed.Select(e =>
                $"{e.FundName ?? "Unknown"} ({e.Period:MMM-yyyy}): forecast {FormatM(e.ForecastDistributions)}, actual {FormatM(e.ActualDistributions)} (shortfall {FormatM(e.ForecastDistributions - e.ActualDistributions)})"))
            : "No material distribution delays identified.";

        // Higher-than-expected calls: actual calls > 120% of forecast
        var higherCalls = entries
            .Where(e => e.ForecastCapitalCalls > 0 && e.ActualCapitalCalls > e.ForecastCapitalCalls * 1.2m)
            .OrderByDescending(e => e.ActualCapitalCalls - e.ForecastCapitalCalls)
            .Take(settings.TopDriverCount)
            .ToList();
        var callsText = higherCalls.Any()
            ? "Funds with higher-than-expected capital calls: " + string.Join("; ", higherCalls.Select(e =>
                $"{e.FundName ?? "Unknown"} ({e.Period:MMM-yyyy}): forecast {FormatM(e.ForecastCapitalCalls)}, actual {FormatM(e.ActualCapitalCalls)} (excess {FormatM(e.ActualCapitalCalls - e.ForecastCapitalCalls)})"))
            : "No material excess capital calls identified.";

        // Watchpoints: items whose variance exceeds the org's watchpoint threshold (% of total forecast net)
        var watchpointFraction = settings.WatchpointThresholdPct / 100m;
        var bigVariances = entries
            .Where(e => e.ForecastCapitalCalls + e.ForecastDistributions > 0 && totalForecastNet != 0)
            .Where(e => Math.Abs(e.NetCashflowVariance) > Math.Abs(totalForecastNet) * watchpointFraction)
            .OrderByDescending(e => Math.Abs(e.NetCashflowVariance))
            .Take(3)
            .ToList();
        var watchText = bigVariances.Any()
            ? $"Watchpoints — items with variance exceeding {settings.WatchpointThresholdPct:0.#}% of total forecast net cashflow: " +
              string.Join("; ", bigVariances.Select(w => $"{w.FundName ?? "Unknown"} ({w.Period:MMM-yyyy}): net variance {FormatM(w.NetCashflowVariance)}"))
            : $"No individual fund-period variances exceeded the {settings.WatchpointThresholdPct:0.#}% materiality threshold.";

        return new CommentaryContentDto
        {
            ExecutiveSummary = execSummary,
            PositiveVariances = posText,
            NegativeVariances = negText,
            DelayedDistributions = delayedText,
            HigherThanExpectedCalls = callsText,
            Watchpoints = watchText
        };
    }

    private static string FormatM(decimal value) => $"${value:N0}";
    private static string FormatPct(decimal value) => $"{value:+0.##;-0.##;0}%";
}
