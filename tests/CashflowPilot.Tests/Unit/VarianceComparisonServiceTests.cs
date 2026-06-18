using CashflowPilot.Domain.Entities;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CashflowPilot.Tests.Unit;

public class VarianceComparisonServiceTests
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
    public async Task CompareForecastToActual_ComputesTotalsAndDeltas()
    {
        var (db, orgId, portfolioId, forecastRunId, actualRunId) = SetupBasicRuns();

        db.CashflowEntries.AddRange(
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = forecastRunId, PortfolioId = portfolioId, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Forecast, CapitalCalls = 1_000_000, Distributions = 200_000, Currency = "USD" },
            new CashflowEntry { OrganizationId = orgId, ActualRunId = actualRunId, PortfolioId = portfolioId, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Actual, CapitalCalls = 1_100_000, Distributions = 150_000, Currency = "USD" }
        );
        db.SaveChanges();

        var service = new VarianceComparisonService(db, new ScenarioService(db));
        var result = await service.CompareForecastToActual(forecastRunId, actualRunId, orgId);

        result.BasisLabel.Should().Be("Forecast vs Actual");
        result.LeftTotalCalls.Should().Be(1_000_000);
        result.RightTotalCalls.Should().Be(1_100_000);
        result.CallsDelta.Should().Be(100_000);
        result.LeftTotalDistributions.Should().Be(200_000);
        result.RightTotalDistributions.Should().Be(150_000);
        result.DistributionsDelta.Should().Be(-50_000);
        result.ByPeriod.Should().HaveCount(1);
        result.ByPeriod[0].LeftNet.Should().Be(-800_000);
        result.ByPeriod[0].RightNet.Should().Be(-950_000);
    }

    [Fact]
    public async Task CompareForecastRuns_ComputesDeltaBetweenRuns()
    {
        var (db, orgId, portfolioId, previousForecastRunId, _) = SetupBasicRuns();

        var currentForecastRun = new ForecastRun
        {
            OrganizationId = orgId, PortfolioId = portfolioId, Name = "Q2 2024 Forecast",
            RunDate = DateTime.UtcNow, CreatedByUserId = "user1", UploadedFileId = db.UploadedFiles.First().Id, Status = RunStatus.Completed
        };
        db.ForecastRuns.Add(currentForecastRun);
        db.SaveChanges();

        db.CashflowEntries.AddRange(
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = previousForecastRunId, PortfolioId = portfolioId, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Forecast, CapitalCalls = 500_000, Distributions = 0, Currency = "USD" },
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = currentForecastRun.Id, PortfolioId = portfolioId, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Forecast, CapitalCalls = 650_000, Distributions = 0, Currency = "USD" }
        );
        db.SaveChanges();

        var service = new VarianceComparisonService(db, new ScenarioService(db));
        var result = await service.CompareForecastRuns(currentForecastRun.Id, previousForecastRunId, orgId);

        result.BasisLabel.Should().Be("Forecast Run vs Previous Forecast Run");
        result.LeftTotalCalls.Should().Be(500_000);
        result.RightTotalCalls.Should().Be(650_000);
        result.CallsDelta.Should().Be(150_000);
    }

    [Fact]
    public async Task CompareScenarioToBase_ComputesDeltaFromAdjustments()
    {
        var (db, orgId, _, forecastRunId, _) = SetupBasicRuns();

        db.CashflowEntries.AddRange(
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = forecastRunId, PortfolioId = db.Portfolios.First().Id, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Forecast, CapitalCalls = 1_000_000, Distributions = 0, Currency = "USD" },
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = forecastRunId, PortfolioId = db.Portfolios.First().Id, FundName = "Fund A", Period = new DateTime(2024, 2, 1), EntryType = EntryType.Forecast, CapitalCalls = 0, Distributions = 500_000, Currency = "USD" }
        );
        db.SaveChanges();

        var baseScenario = new Scenario
        {
            OrganizationId = orgId, ForecastRunId = forecastRunId, Name = "Base Case",
            CallsAdjustmentPct = 0, DistributionsAdjustmentPct = 0, TimingShiftMonths = 0, CreatedByUserId = "user1"
        };
        var stressScenario = new Scenario
        {
            OrganizationId = orgId, ForecastRunId = forecastRunId, Name = "Stress Case",
            CallsAdjustmentPct = 10, DistributionsAdjustmentPct = -20, TimingShiftMonths = 0, CreatedByUserId = "user1"
        };
        db.Scenarios.AddRange(baseScenario, stressScenario);
        db.SaveChanges();

        var service = new VarianceComparisonService(db, new ScenarioService(db));
        var result = await service.CompareScenarioToBase(stressScenario.Id, baseScenario.Id, orgId);

        result.BasisLabel.Should().Be("Scenario vs Scenario");
        result.LeftLabel.Should().Be("Base Case");
        result.RightLabel.Should().Be("Stress Case");
        result.LeftTotalCalls.Should().Be(1_000_000);
        result.RightTotalCalls.Should().Be(1_100_000);
        result.LeftTotalDistributions.Should().Be(500_000);
        result.RightTotalDistributions.Should().Be(400_000);
    }
}
