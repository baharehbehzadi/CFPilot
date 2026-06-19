using CashflowPilot.Application.DTOs;
using CashflowPilot.Domain.Entities;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CashflowPilot.Tests.Unit;

public class ScenarioCalculationTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static (ApplicationDbContext db, int orgId, int portfolioId, int forecastRunId) SetupForecastRun()
    {
        var db = CreateDb();
        var org = new Organization { Name = "Test Org", CreatedAt = DateTime.UtcNow };
        db.Organizations.Add(org);
        db.SaveChanges();
        var portfolio = new Portfolio { OrganizationId = org.Id, Name = "Test Portfolio", CreatedAt = DateTime.UtcNow };
        db.Portfolios.Add(portfolio);
        db.SaveChanges();
        var uploadFile = new UploadedFile
        {
            OrganizationId = org.Id, OriginalFileName = "f.csv", StoredFileName = "f.csv",
            FilePath = "/tmp/f.csv", ContentType = "text/csv",
            FileType = FileType.ForecastCsv, Status = FileStatus.Processed
        };
        db.UploadedFiles.Add(uploadFile);
        db.SaveChanges();
        var run = new ForecastRun
        {
            OrganizationId = org.Id, PortfolioId = portfolio.Id, Name = "Forecast",
            RunDate = DateTime.UtcNow, CreatedByUserId = "u",
            UploadedFileId = uploadFile.Id, Status = RunStatus.Completed
        };
        db.ForecastRuns.Add(run);
        db.SaveChanges();
        return (db, org.Id, portfolio.Id, run.Id);
    }

    [Fact]
    public async Task Preview_NoAdjustment_ReturnsSameAsBaseline()
    {
        var (db, orgId, portfolioId, runId) = SetupForecastRun();
        db.CashflowEntries.Add(new CashflowEntry
        {
            OrganizationId = orgId, ForecastRunId = runId, PortfolioId = portfolioId,
            FundName = "Fund A", Period = new DateTime(2024, 1, 1),
            EntryType = EntryType.Forecast, CapitalCalls = 1_000_000, Distributions = 200_000, Currency = "USD"
        });
        db.SaveChanges();

        var service = new ScenarioService(db);
        var result = await service.PreviewScenarioAsync(runId, new ScenarioAssumptionsDto(), orgId);

        result.Periods.Should().HaveCount(1);
        result.Periods[0].ScenarioCalls.Should().Be(result.Periods[0].BaselineCalls);
        result.Periods[0].ScenarioDistributions.Should().Be(result.Periods[0].BaselineDistributions);
        result.NetCashflowDelta.Should().Be(0);
    }

    [Fact]
    public async Task Preview_PositiveCallsAdjustment_IncreasesCallsAndReducesNet()
    {
        var (db, orgId, portfolioId, runId) = SetupForecastRun();
        db.CashflowEntries.Add(new CashflowEntry
        {
            OrganizationId = orgId, ForecastRunId = runId, PortfolioId = portfolioId,
            FundName = "Fund A", Period = new DateTime(2024, 1, 1),
            EntryType = EntryType.Forecast, CapitalCalls = 1_000_000, Distributions = 0, Currency = "USD"
        });
        db.SaveChanges();

        var service = new ScenarioService(db);
        var result = await service.PreviewScenarioAsync(runId, new ScenarioAssumptionsDto { CallsAdjustmentPct = 10 }, orgId); // +10% calls

        result.Periods[0].ScenarioCalls.Should().Be(1_100_000m);
        result.NetCashflowDelta.Should().Be(-100_000); // more calls = worse net
    }

    [Fact]
    public async Task Preview_PositiveDistAdjustment_ImprovesNet()
    {
        var (db, orgId, portfolioId, runId) = SetupForecastRun();
        db.CashflowEntries.Add(new CashflowEntry
        {
            OrganizationId = orgId, ForecastRunId = runId, PortfolioId = portfolioId,
            FundName = "Fund A", Period = new DateTime(2024, 1, 1),
            EntryType = EntryType.Forecast, CapitalCalls = 0, Distributions = 1_000_000, Currency = "USD"
        });
        db.SaveChanges();

        var service = new ScenarioService(db);
        var result = await service.PreviewScenarioAsync(runId, new ScenarioAssumptionsDto { DistributionsAdjustmentPct = 20 }, orgId); // +20% distributions

        result.Periods[0].ScenarioDistributions.Should().Be(1_200_000m);
        result.NetCashflowDelta.Should().Be(200_000);
    }

    [Fact]
    public async Task Preview_TimingShift_MovesPeriodsForward()
    {
        var (db, orgId, portfolioId, runId) = SetupForecastRun();
        db.CashflowEntries.Add(new CashflowEntry
        {
            OrganizationId = orgId, ForecastRunId = runId, PortfolioId = portfolioId,
            FundName = "Fund A", Period = new DateTime(2024, 1, 1),
            EntryType = EntryType.Forecast, CapitalCalls = 500_000, Distributions = 0, Currency = "USD"
        });
        db.SaveChanges();

        var service = new ScenarioService(db);
        var result = await service.PreviewScenarioAsync(runId,
            new ScenarioAssumptionsDto { CallsTimingShiftMonths = 2, DistributionsTimingShiftMonths = 2 }, orgId); // shift +2 months

        // Baseline is Jan 2024, scenario shifts to Mar 2024
        var scenarioPeriod = result.Periods.FirstOrDefault(p => p.ScenarioCalls > 0);
        scenarioPeriod.Should().NotBeNull();
        scenarioPeriod!.Period.Should().Be(new DateTime(2024, 3, 1));
    }

    [Fact]
    public async Task Preview_NegativeCallsAdjustment_ImprovesNet()
    {
        var (db, orgId, portfolioId, runId) = SetupForecastRun();
        db.CashflowEntries.Add(new CashflowEntry
        {
            OrganizationId = orgId, ForecastRunId = runId, PortfolioId = portfolioId,
            FundName = "Fund A", Period = new DateTime(2024, 1, 1),
            EntryType = EntryType.Forecast, CapitalCalls = 1_000_000, Distributions = 0, Currency = "USD"
        });
        db.SaveChanges();

        var service = new ScenarioService(db);
        var result = await service.PreviewScenarioAsync(runId, new ScenarioAssumptionsDto { CallsAdjustmentPct = -10 }, orgId); // -10% calls

        result.Periods[0].ScenarioCalls.Should().Be(900_000m);
        result.NetCashflowDelta.Should().Be(100_000); // fewer calls = better net
    }

    [Fact]
    public async Task Preview_MultiPeriod_CombinedAdjustments_ComputeCorrectly()
    {
        var (db, orgId, portfolioId, runId) = SetupForecastRun();
        db.CashflowEntries.AddRange(
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = runId, PortfolioId = portfolioId, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Forecast, CapitalCalls = 1_000_000, Distributions = 0, Currency = "USD" },
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = runId, PortfolioId = portfolioId, FundName = "Fund A", Period = new DateTime(2024, 2, 1), EntryType = EntryType.Forecast, CapitalCalls = 0, Distributions = 500_000, Currency = "USD" }
        );
        db.SaveChanges();

        var service = new ScenarioService(db);
        var result = await service.PreviewScenarioAsync(runId,
            new ScenarioAssumptionsDto { CallsAdjustmentPct = 10, DistributionsAdjustmentPct = 10 }, orgId);

        // Jan: calls 1M → 1.1M, dist 0 → 0
        // Feb: calls 0 → 0, dist 500k → 550k
        var jan = result.Periods.First(p => p.Period.Month == 1);
        var feb = result.Periods.First(p => p.Period.Month == 2);
        jan.ScenarioCalls.Should().Be(1_100_000m);
        feb.ScenarioDistributions.Should().Be(550_000m);

        // Net delta: calls increased by 100k (bad), dist increased by 50k (good) → net delta = -50k
        result.NetCashflowDelta.Should().Be(-50_000m);
    }

    [Fact]
    public async Task Preview_IndependentTimingShifts_MoveCallsAndDistributionsToDifferentPeriods()
    {
        var (db, orgId, portfolioId, runId) = SetupForecastRun();
        db.CashflowEntries.Add(new CashflowEntry
        {
            OrganizationId = orgId, ForecastRunId = runId, PortfolioId = portfolioId,
            FundName = "Fund A", Period = new DateTime(2024, 1, 1),
            EntryType = EntryType.Forecast, CapitalCalls = 500_000, Distributions = 200_000, Currency = "USD"
        });
        db.SaveChanges();

        var service = new ScenarioService(db);
        var result = await service.PreviewScenarioAsync(runId,
            new ScenarioAssumptionsDto { CallsTimingShiftMonths = 1, DistributionsTimingShiftMonths = 3 }, orgId);

        // Calls shift to Feb 2024, distributions shift to Apr 2024 — independent of each other.
        var feb = result.Periods.First(p => p.Period == new DateTime(2024, 2, 1));
        var apr = result.Periods.First(p => p.Period == new DateTime(2024, 4, 1));
        feb.ScenarioCalls.Should().Be(500_000m);
        feb.ScenarioDistributions.Should().Be(0m);
        apr.ScenarioDistributions.Should().Be(200_000m);
        apr.ScenarioCalls.Should().Be(0m);
    }

    [Fact]
    public async Task Preview_FundScope_OnlyAdjustsEntriesForTargetFund()
    {
        var (db, orgId, portfolioId, runId) = SetupForecastRun();
        db.CashflowEntries.AddRange(
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = runId, PortfolioId = portfolioId, FundId = 1, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Forecast, CapitalCalls = 1_000_000, Distributions = 0, Currency = "USD" },
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = runId, PortfolioId = portfolioId, FundId = 2, FundName = "Fund B", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Forecast, CapitalCalls = 1_000_000, Distributions = 0, Currency = "USD" }
        );
        db.SaveChanges();

        var service = new ScenarioService(db);
        var result = await service.PreviewScenarioAsync(runId,
            new ScenarioAssumptionsDto { CallsAdjustmentPct = 10, Scope = "Fund", ScopeFundId = 1 }, orgId);

        // Fund A (in scope) gets +10%, Fund B (out of scope) stays at baseline.
        var jan = result.Periods.Single(p => p.Period == new DateTime(2024, 1, 1));
        jan.BaselineCalls.Should().Be(2_000_000m);
        jan.ScenarioCalls.Should().Be(2_100_000m); // 1.1M (Fund A) + 1M (Fund B unchanged)
    }

    [Fact]
    public async Task Preview_StrategyScope_OnlyAdjustsEntriesForTargetStrategy()
    {
        var (db, orgId, portfolioId, runId) = SetupForecastRun();
        db.CashflowEntries.AddRange(
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = runId, PortfolioId = portfolioId, StrategyId = 5, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Forecast, CapitalCalls = 0, Distributions = 1_000_000, Currency = "USD" },
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = runId, PortfolioId = portfolioId, StrategyId = 9, FundName = "Fund B", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Forecast, CapitalCalls = 0, Distributions = 1_000_000, Currency = "USD" }
        );
        db.SaveChanges();

        var service = new ScenarioService(db);
        var result = await service.PreviewScenarioAsync(runId,
            new ScenarioAssumptionsDto { DistributionsAdjustmentPct = -50, Scope = "Strategy", ScopeStrategyId = 5 }, orgId);

        var jan = result.Periods.Single(p => p.Period == new DateTime(2024, 1, 1));
        jan.BaselineDistributions.Should().Be(2_000_000m);
        jan.ScenarioDistributions.Should().Be(1_500_000m); // 500k (Strategy 5, halved) + 1M (Strategy 9 unchanged)
    }
}
