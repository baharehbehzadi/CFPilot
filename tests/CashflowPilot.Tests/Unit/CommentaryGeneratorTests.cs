using CashflowPilot.Domain.Entities;
using CashflowPilot.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace CashflowPilot.Tests.Unit;

public class CommentaryGeneratorTests
{
    private static VarianceAnalysis CreateAnalysis() => new()
    {
        ActualRun = new ActualRun { Name = "Q1 2024 Actual" }
    };

    private static OrganizationSettings DefaultSettings() => new();

    [Fact]
    public void Generate_MaterialPositiveVariance_IsIncluded()
    {
        var entries = new List<VarianceEntry>
        {
            new() { FundName = "Fund A", Period = new DateTime(2024, 1, 1), ForecastCapitalCalls = 0, ActualCapitalCalls = 0, ForecastDistributions = 1_000_000, ActualDistributions = 1_500_000 }
        };

        var result = new RuleBasedCommentaryGenerator().Generate(CreateAnalysis(), entries, DefaultSettings());

        result.PositiveVariances.Should().Contain("Fund A");
        result.PositiveVariances.Should().NotBe("No material positive variances recorded in this period.");
    }

    [Fact]
    public void Generate_ImmaterialVariance_IsExcludedFromPositiveAndNegative()
    {
        // Below both the $250k absolute threshold and the 10% relative threshold.
        var entries = new List<VarianceEntry>
        {
            new() { FundName = "Fund A", Period = new DateTime(2024, 1, 1), ForecastCapitalCalls = 0, ActualCapitalCalls = 0, ForecastDistributions = 10_000_000, ActualDistributions = 10_050_000 }
        };

        var result = new RuleBasedCommentaryGenerator().Generate(CreateAnalysis(), entries, DefaultSettings());

        result.PositiveVariances.Should().Be("No material positive variances recorded in this period.");
    }

    [Fact]
    public void Generate_TopDriverCount_LimitsNumberOfItemsListed()
    {
        var entries = new List<VarianceEntry>();
        for (int i = 0; i < 5; i++)
        {
            entries.Add(new VarianceEntry
            {
                FundName = $"Fund {i}",
                Period = new DateTime(2024, 1, 1),
                ForecastDistributions = 1_000_000,
                ActualDistributions = 1_000_000 + (i + 1) * 300_000
            });
        }

        var settings = new OrganizationSettings { TopDriverCount = 2 };
        var result = new RuleBasedCommentaryGenerator().Generate(CreateAnalysis(), entries, settings);

        // Should only mention the top 2 by variance magnitude (Fund 4 and Fund 3).
        result.PositiveVariances.Should().Contain("Fund 4");
        result.PositiveVariances.Should().Contain("Fund 3");
        result.PositiveVariances.Should().NotContain("Fund 0");
    }

    [Fact]
    public void Generate_WatchpointThreshold_IsConfigurable()
    {
        // Net variance is 20% of total forecast net.
        var entries = new List<VarianceEntry>
        {
            new() { FundName = "Fund A", Period = new DateTime(2024, 1, 1), ForecastDistributions = 1_000_000, ActualDistributions = 1_200_000 }
        };

        var lenientSettings = new OrganizationSettings { WatchpointThresholdPct = 30m };
        var lenientResult = new RuleBasedCommentaryGenerator().Generate(CreateAnalysis(), entries, lenientSettings);
        lenientResult.Watchpoints.Should().Contain("30%");
        lenientResult.Watchpoints.Should().NotContain("Fund A");

        var strictSettings = new OrganizationSettings { WatchpointThresholdPct = 10m };
        var strictResult = new RuleBasedCommentaryGenerator().Generate(CreateAnalysis(), entries, strictSettings);
        strictResult.Watchpoints.Should().Contain("Fund A");
    }

    [Fact]
    public void Generate_DelayedDistributions_DetectsShortfallBelowHalfForecast()
    {
        var entries = new List<VarianceEntry>
        {
            new() { FundName = "Fund A", Period = new DateTime(2024, 1, 1), ForecastDistributions = 1_000_000, ActualDistributions = 400_000 }
        };

        var result = new RuleBasedCommentaryGenerator().Generate(CreateAnalysis(), entries, DefaultSettings());

        result.DelayedDistributions.Should().Contain("Fund A");
    }

    [Fact]
    public void Generate_HigherThanExpectedCalls_DetectsExcessAbove120Pct()
    {
        var entries = new List<VarianceEntry>
        {
            new() { FundName = "Fund A", Period = new DateTime(2024, 1, 1), ForecastCapitalCalls = 1_000_000, ActualCapitalCalls = 1_300_000 }
        };

        var result = new RuleBasedCommentaryGenerator().Generate(CreateAnalysis(), entries, DefaultSettings());

        result.HigherThanExpectedCalls.Should().Contain("Fund A");
    }

    [Fact]
    public void Generate_ExecutiveSummary_ReportsCorrectTotalsAndCount()
    {
        var entries = new List<VarianceEntry>
        {
            new() { FundName = "Fund A", Period = new DateTime(2024, 1, 1), ForecastDistributions = 1_000_000, ActualDistributions = 1_200_000 },
            new() { FundName = "Fund B", Period = new DateTime(2024, 1, 1), ForecastCapitalCalls = 500_000, ActualCapitalCalls = 500_000 }
        };

        var result = new RuleBasedCommentaryGenerator().Generate(CreateAnalysis(), entries, DefaultSettings());

        result.ExecutiveSummary.Should().Contain("Q1 2024 Actual");
        result.ExecutiveSummary.Should().Contain("2 fund-period combinations");
        result.ExecutiveSummary.Should().Contain("positive net cashflow variance");
    }

    [Fact]
    public void Generate_NoEntries_ReturnsNonMaterialDefaultsForAllSections()
    {
        var result = new RuleBasedCommentaryGenerator().Generate(CreateAnalysis(), new List<VarianceEntry>(), DefaultSettings());

        result.PositiveVariances.Should().Be("No material positive variances recorded in this period.");
        result.NegativeVariances.Should().Be("No material negative variances recorded in this period.");
        result.DelayedDistributions.Should().Be("No material distribution delays identified.");
        result.HigherThanExpectedCalls.Should().Be("No material excess capital calls identified.");
    }
}
