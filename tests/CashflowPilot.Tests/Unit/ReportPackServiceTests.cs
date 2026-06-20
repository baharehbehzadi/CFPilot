using CashflowPilot.Domain.Entities;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CashflowPilot.Tests.Unit;

public class ReportPackServiceTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static (ApplicationDbContext db, int orgId, int portfolioId, int forecastRunId, int actualRunId, int forecastFileId, int actualFileId) SetupRuns()
    {
        var db = CreateDb();
        var org = new Organization { Name = "Test Org", CreatedAt = DateTime.UtcNow };
        db.Organizations.Add(org);
        db.SaveChanges();

        var portfolio = new Portfolio { OrganizationId = org.Id, Name = "Test Portfolio", CreatedAt = DateTime.UtcNow };
        db.Portfolios.Add(portfolio);
        db.SaveChanges();

        var forecastFile = new UploadedFile
        {
            OrganizationId = org.Id, OriginalFileName = "forecast.csv", StoredFileName = "forecast.csv",
            FilePath = "/tmp/forecast.csv", ContentType = "text/csv",
            FileType = FileType.ForecastCsv, Status = FileStatus.Processed
        };
        var actualFile = new UploadedFile
        {
            OrganizationId = org.Id, OriginalFileName = "actual.csv", StoredFileName = "actual.csv",
            FilePath = "/tmp/actual.csv", ContentType = "text/csv",
            FileType = FileType.ActualCsv, Status = FileStatus.Processed
        };
        db.UploadedFiles.AddRange(forecastFile, actualFile);
        db.SaveChanges();

        var forecastRun = new ForecastRun
        {
            OrganizationId = org.Id, PortfolioId = portfolio.Id, Name = "Q1 2024 Forecast",
            RunDate = DateTime.UtcNow, CreatedByUserId = "user1", UploadedFileId = forecastFile.Id, Status = RunStatus.Completed
        };
        db.ForecastRuns.Add(forecastRun);
        db.SaveChanges();

        var actualRun = new ActualRun
        {
            OrganizationId = org.Id, PortfolioId = portfolio.Id, Name = "Q1 2024 Actual",
            ReportingPeriod = new DateTime(2024, 3, 1), RunDate = DateTime.UtcNow,
            CreatedByUserId = "user1", UploadedFileId = actualFile.Id, Status = RunStatus.Completed
        };
        db.ActualRuns.Add(actualRun);
        db.SaveChanges();

        return (db, org.Id, portfolio.Id, forecastRun.Id, actualRun.Id, forecastFile.Id, actualFile.Id);
    }

    [Fact]
    public async Task BuildAsync_ReturnsKpiSummaryAndTopDrivers()
    {
        var (db, orgId, portfolioId, forecastRunId, actualRunId, _, _) = SetupRuns();
        db.CashflowEntries.AddRange(
            new CashflowEntry { OrganizationId = orgId, ForecastRunId = forecastRunId, PortfolioId = portfolioId, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Forecast, CapitalCalls = 1_000_000, Distributions = 0, Currency = "USD" },
            new CashflowEntry { OrganizationId = orgId, ActualRunId = actualRunId, PortfolioId = portfolioId, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Actual, CapitalCalls = 1_200_000, Distributions = 0, Currency = "USD" }
        );
        db.SaveChanges();

        var variance = new VarianceService(db);
        var analysisId = await variance.CreateAnalysisAsync(forecastRunId, actualRunId, "user1", orgId);

        var service = new ReportPackService(db, variance, new ScenarioService(db));
        var pack = await service.BuildAsync(analysisId, orgId);

        pack.Summary.TotalCallsVariance.Should().Be(200_000);
        pack.TopFundDrivers.Should().ContainSingle(f => f.FundName == "Fund A");
        pack.TopPeriodDrivers.Should().ContainSingle(p => p.Period == new DateTime(2024, 1, 1));
    }

    [Fact]
    public async Task BuildAsync_IncludesScenariosForForecastRun()
    {
        var (db, orgId, portfolioId, forecastRunId, actualRunId, _, _) = SetupRuns();
        db.CashflowEntries.Add(new CashflowEntry
        {
            OrganizationId = orgId, ForecastRunId = forecastRunId, PortfolioId = portfolioId,
            FundName = "Fund A", Period = new DateTime(2024, 1, 1),
            EntryType = EntryType.Forecast, CapitalCalls = 1_000_000, Distributions = 0, Currency = "USD"
        });
        db.Scenarios.Add(new Scenario
        {
            OrganizationId = orgId, ForecastRunId = forecastRunId, Name = "Bear Case",
            CallsAdjustmentPct = 10, CreatedAt = DateTime.UtcNow, CreatedByUserId = "user1"
        });
        db.SaveChanges();

        var variance = new VarianceService(db);
        var analysisId = await variance.CreateAnalysisAsync(forecastRunId, actualRunId, "user1", orgId);

        var service = new ReportPackService(db, variance, new ScenarioService(db));
        var pack = await service.BuildAsync(analysisId, orgId);

        pack.Scenarios.Should().ContainSingle(s => s.ScenarioName == "Bear Case");
        pack.Scenarios[0].NetCashflowDelta.Should().Be(-100_000);
    }

    [Fact]
    public async Task BuildAsync_IncludesDataQualityIssuesForSourceFiles()
    {
        var (db, orgId, portfolioId, forecastRunId, actualRunId, forecastFileId, actualFileId) = SetupRuns();
        db.ValidationIssues.Add(new ValidationIssue
        {
            OrganizationId = orgId, UploadedFileId = forecastFileId, Severity = ValidationSeverity.Warning,
            RowNumber = 5, Message = "Missing currency code"
        });
        db.ValidationIssues.Add(new ValidationIssue
        {
            OrganizationId = orgId, UploadedFileId = 9999, Severity = ValidationSeverity.Error,
            Message = "Unrelated file issue"
        });
        db.SaveChanges();

        var variance = new VarianceService(db);
        var analysisId = await variance.CreateAnalysisAsync(forecastRunId, actualRunId, "user1", orgId);

        var service = new ReportPackService(db, variance, new ScenarioService(db));
        var pack = await service.BuildAsync(analysisId, orgId);

        pack.DataQualityIssues.Should().ContainSingle(i => i.Message == "Missing currency code");
    }

    [Fact]
    public async Task BuildAsync_IncludesAuditEntriesScopedToAnalysis()
    {
        var (db, orgId, portfolioId, forecastRunId, actualRunId, _, _) = SetupRuns();
        db.SaveChanges();

        var variance = new VarianceService(db);
        var analysisId = await variance.CreateAnalysisAsync(forecastRunId, actualRunId, "user1", orgId);

        db.AuditLogs.Add(new AuditLog
        {
            OrganizationId = orgId, UserId = "user1", UserName = "Analyst One", Action = "AnalysisRun",
            EntityType = "VarianceAnalysis", EntityId = analysisId.ToString(), Details = "Created analysis"
        });
        db.AuditLogs.Add(new AuditLog
        {
            OrganizationId = orgId, UserId = "user1", UserName = "Analyst One", Action = "Export",
            EntityType = "VarianceAnalysis", EntityId = (analysisId + 999).ToString(), Details = "Unrelated export"
        });
        db.SaveChanges();

        var service = new ReportPackService(db, variance, new ScenarioService(db));
        var pack = await service.BuildAsync(analysisId, orgId);

        pack.AuditEntries.Should().ContainSingle(a => a.Details == "Created analysis");
    }
}
