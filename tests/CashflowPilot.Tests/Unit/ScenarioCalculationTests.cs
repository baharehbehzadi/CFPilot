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
        var result = await service.PreviewScenarioAsync(runId, 0, 0, 0, orgId);

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
        var result = await service.PreviewScenarioAsync(runId, 10, 0, 0, orgId); // +10% calls

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
        var result = await service.PreviewScenarioAsync(runId, 0, 20, 0, orgId); // +20% distributions

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
        var result = await service.PreviewScenarioAsync(runId, 0, 0, 2, orgId); // shift +2 months

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
        var result = await service.PreviewScenarioAsync(runId, -10, 0, 0, orgId); // -10% calls

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
        var result = await service.PreviewScenarioAsync(runId, 10, 10, 0, orgId);

        // Jan: calls 1M → 1.1M, dist 0 → 0
        // Feb: calls 0 → 0, dist 500k → 550k
        var jan = result.Periods.First(p => p.Period.Month == 1);
        var feb = result.Periods.First(p => p.Period.Month == 2);
        jan.ScenarioCalls.Should().Be(1_100_000m);
        feb.ScenarioDistributions.Should().Be(550_000m);

        // Net delta: calls increased by 100k (bad), dist increased by 50k (good) → net delta = -50k
        result.NetCashflowDelta.Should().Be(-50_000m);
    }
}
