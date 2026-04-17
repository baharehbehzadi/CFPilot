using CashflowPilot.Domain.Entities;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CashflowPilot.Tests.Unit;

public class VarianceCalculationTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static (ApplicationDbContext db, int orgId, int portfolioId, int forecastRunId, int actualRunId) SetupBasicRuns()
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
            OrganizationId = org.Id, OriginalFileName = "test.csv", StoredFileName = "test.csv",
            FilePath = "/tmp/test.csv", ContentType = "text/csv",
            FileType = FileType.ForecastCsv, Status = FileStatus.Processed
        };
        db.UploadedFiles.Add(uploadFile);
        db.SaveChanges();

        var forecastRun = new ForecastRun
        {
            OrganizationId = org.Id, PortfolioId = portfolio.Id, Name = "Q1 2024 Forecast",
            RunDate = DateTime.UtcNow, CreatedByUserId = "user1", UploadedFileId = uploadFile.Id, Status = RunStatus.Completed
        };
        db.ForecastRuns.Add(forecastRun);
        db.SaveChanges();

        var actualRun = new ActualRun
        {
            OrganizationId = org.Id, PortfolioId = portfolio.Id, Name = "Q1 2024 Actual",
            ReportingPeriod = new DateTime(2024, 3, 1), RunDate = DateTime.UtcNow,
            CreatedByUserId = "user1", UploadedFileId = uploadFile.Id, Status = RunStatus.Completed
        };
        db.ActualRuns.Add(actualRun);
        db.SaveChanges();

        return (db, org.Id, portfolio.Id, forecastRun.Id, actualRun.Id);
    }

    [Fact]
    public async Task CreateAnalysis_BasicVariance_ComputesCorrectly()
    {
        var (db, orgId, portfolioId, forecastRunId, actualRunId) = SetupBasicRuns();

        db.CashflowEntries.Add(new CashflowEntry
        {
            OrganizationId = orgId, ForecastRunId = forecastRunId, PortfolioId = portfolioId,
            FundName = "Apex Fund", Period = new DateTime(2024, 1, 1),
            EntryType = EntryType.Forecast, CapitalCalls = 1_000_000, Distributions = 0, Currency = "USD"
        });
        db.CashflowEntries.Add(new CashflowEntry
        {
            OrganizationId = orgId, ActualRunId = actualRunId, PortfolioId = portfolioId,
            FundName = "Apex Fund", Period = new DateTime(2024, 1, 1),
            EntryType = EntryType.Actual, CapitalCalls = 1_200_000, Distributions = 0, Currency = "USD"
        });
        db.SaveChanges();

        var service = new VarianceService(db);
        var analysisId = await service.CreateAnalysisAsync(forecastRunId, actualRunId, "user1", orgId);
        var summary = await service.GetSummaryAsync(analysisId, orgId);

        summary.Should().NotBeNull();
        summary!.TotalForecastCalls.Should().Be(1_000_000);
        summary.TotalActualCalls.Should().Be(1_200_000);
        summary.TotalCallsVariance.Should().Be(200_000);
        summary.TotalNetCashflowVariance.Should().Be(-200_000);
    }

    [Fact]
    public async Task CreateAnalysis_CumulativeVariance_IsRunningSum()
    {
        var (db, orgId, portfolioId, forecastRunId, actualRunId) = SetupBasicRuns();

        db.CashflowEntries.AddRange(
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = forecastRunId, PortfolioId = portfolioId, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Forecast, CapitalCalls = 100_000, Distributions = 0, Currency = "USD" },
            new CashflowEntry { OrganizationId = orgId, ActualRunId = actualRunId, PortfolioId = portfolioId, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Actual, CapitalCalls = 80_000, Distributions = 0, Currency = "USD" },
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = forecastRunId, PortfolioId = portfolioId, FundName = "Fund A", Period = new DateTime(2024, 2, 1), EntryType = EntryType.Forecast, CapitalCalls = 200_000, Distributions = 0, Currency = "USD" },
            new CashflowEntry { OrganizationId = orgId, ActualRunId = actualRunId, PortfolioId = portfolioId, FundName = "Fund A", Period = new DateTime(2024, 2, 1), EntryType = EntryType.Actual, CapitalCalls = 240_000, Distributions = 0, Currency = "USD" }
        );
        db.SaveChanges();

        var service = new VarianceService(db);
        var analysisId = await service.CreateAnalysisAsync(forecastRunId, actualRunId, "user1", orgId);
        var summary = await service.GetSummaryAsync(analysisId, orgId);

        // Jan: actual net=-80k, forecast net=-100k, variance=+20k, cumulative=+20k
        // Feb: actual net=-240k, forecast net=-200k, variance=-40k, cumulative=-20k
        summary!.ByPeriod[0].NetVariance.Should().Be(20_000);
        summary.ByPeriod[0].CumulativeVariance.Should().Be(20_000);
        summary.ByPeriod[1].NetVariance.Should().Be(-40_000);
        summary.ByPeriod[1].CumulativeVariance.Should().Be(-20_000);
    }

    [Fact]
    public async Task GetSummary_NoEntries_ReturnsZeroTotals()
    {
        var (db, orgId, _, forecastRunId, actualRunId) = SetupBasicRuns();
        var service = new VarianceService(db);
        var analysisId = await service.CreateAnalysisAsync(forecastRunId, actualRunId, "user1", orgId);
        var summary = await service.GetSummaryAsync(analysisId, orgId);

        summary.Should().NotBeNull();
        summary!.TotalForecastCalls.Should().Be(0);
        summary.TotalActualCalls.Should().Be(0);
        summary.TotalNetCashflowVariance.Should().Be(0);
    }

    [Fact]
    public async Task GetSummary_ByFund_GroupsCorrectly()
    {
        var (db, orgId, portfolioId, forecastRunId, actualRunId) = SetupBasicRuns();

        db.CashflowEntries.AddRange(
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = forecastRunId, PortfolioId = portfolioId, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Forecast, CapitalCalls = 500_000, Distributions = 0, Currency = "USD" },
            new CashflowEntry { OrganizationId = orgId, ActualRunId = actualRunId, PortfolioId = portfolioId, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Actual, CapitalCalls = 600_000, Distributions = 0, Currency = "USD" },
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = forecastRunId, PortfolioId = portfolioId, FundName = "Fund B", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Forecast, CapitalCalls = 0, Distributions = 1_000_000, Currency = "USD" },
            new CashflowEntry { OrganizationId = orgId, ActualRunId = actualRunId, PortfolioId = portfolioId, FundName = "Fund B", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Actual, CapitalCalls = 0, Distributions = 800_000, Currency = "USD" }
        );
        db.SaveChanges();

        var service = new VarianceService(db);
        var analysisId = await service.CreateAnalysisAsync(forecastRunId, actualRunId, "user1", orgId);
        var summary = await service.GetSummaryAsync(analysisId, orgId);

        summary!.ByFund.Should().HaveCount(2);
        var fundA = summary.ByFund.First(f => f.FundName == "Fund A");
        fundA.TotalCallsVariance.Should().Be(100_000);
        var fundB = summary.ByFund.First(f => f.FundName == "Fund B");
        fundB.TotalDistributionsVariance.Should().Be(-200_000);
    }

    [Fact]
    public void VarianceEntry_ComputedProperties_AreCorrect()
    {
        var entry = new VarianceEntry
        {
            ForecastCapitalCalls = 1_000_000,
            ActualCapitalCalls = 1_200_000,
            ForecastDistributions = 500_000,
            ActualDistributions = 400_000
        };

        entry.CapitalCallsVariance.Should().Be(200_000);
        entry.CapitalCallsVariancePct.Should().BeApproximately(20m, 0.01m);
        entry.DistributionsVariance.Should().Be(-100_000);
        entry.ForecastNetCashflow.Should().Be(-500_000);
        entry.ActualNetCashflow.Should().Be(-800_000);
        entry.NetCashflowVariance.Should().Be(-300_000);
    }

    [Fact]
    public void VarianceEntry_ZeroForecast_PctIsNull()
    {
        var entry = new VarianceEntry { ForecastCapitalCalls = 0, ActualCapitalCalls = 500_000 };
        entry.CapitalCallsVariancePct.Should().BeNull();
    }
}
