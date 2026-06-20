using CashflowPilot.Domain.Entities;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CashflowPilot.Tests.Unit;

public class CommentaryServiceTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<(ApplicationDbContext db, int orgId, int analysisId)> SetupAnalysis()
    {
        var db = CreateDb();
        var org = new Organization { Name = "Test Org", CreatedAt = DateTime.UtcNow };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        var portfolio = new Portfolio { OrganizationId = org.Id, Name = "Test Portfolio", CreatedAt = DateTime.UtcNow };
        db.Portfolios.Add(portfolio);
        await db.SaveChangesAsync();

        var uploadFile = new UploadedFile
        {
            OrganizationId = org.Id, OriginalFileName = "test.csv", StoredFileName = "test.csv",
            FilePath = "/tmp/test.csv", ContentType = "text/csv", FileType = FileType.ForecastCsv, Status = FileStatus.Processed
        };
        db.UploadedFiles.Add(uploadFile);
        await db.SaveChangesAsync();

        var forecastRun = new ForecastRun
        {
            OrganizationId = org.Id, PortfolioId = portfolio.Id, Name = "Forecast",
            RunDate = DateTime.UtcNow, CreatedByUserId = "user1", UploadedFileId = uploadFile.Id, Status = RunStatus.Completed
        };
        var actualRun = new ActualRun
        {
            OrganizationId = org.Id, PortfolioId = portfolio.Id, Name = "Actual",
            ReportingPeriod = new DateTime(2024, 3, 1), RunDate = DateTime.UtcNow,
            CreatedByUserId = "user1", UploadedFileId = uploadFile.Id, Status = RunStatus.Completed
        };
        db.ForecastRuns.Add(forecastRun);
        db.ActualRuns.Add(actualRun);
        await db.SaveChangesAsync();

        db.CashflowEntries.AddRange(
            new CashflowEntry { OrganizationId = org.Id, ForecastRunId = forecastRun.Id, PortfolioId = portfolio.Id, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Forecast, CapitalCalls = 0, Distributions = 1_000_000, Currency = "USD" },
            new CashflowEntry { OrganizationId = org.Id, ActualRunId = actualRun.Id, PortfolioId = portfolio.Id, FundName = "Fund A", Period = new DateTime(2024, 1, 1), EntryType = EntryType.Actual, CapitalCalls = 0, Distributions = 1_500_000, Currency = "USD" }
        );
        await db.SaveChangesAsync();

        var variance = new VarianceService(db);
        var analysisId = await variance.CreateAnalysisAsync(forecastRun.Id, actualRun.Id, "user1", org.Id);

        return (db, org.Id, analysisId);
    }

    [Fact]
    public async Task GenerateAsync_PersistsCommentaryPackScopedToOrganization()
    {
        var (db, orgId, analysisId) = await SetupAnalysis();
        var service = new CommentaryService(db, new RuleBasedCommentaryGenerator());

        var pack = await service.GenerateAsync(analysisId, "user1", orgId);

        pack.OrganizationId.Should().Be(orgId);
        pack.VarianceAnalysisId.Should().Be(analysisId);
        pack.ExecutiveSummary.Should().Contain("Actual");
        (await db.CommentaryPacks.SingleAsync()).Id.Should().Be(pack.Id);
    }

    [Fact]
    public async Task GenerateAsync_AnalysisNotInOrganization_Throws()
    {
        var (db, orgId, analysisId) = await SetupAnalysis();
        var service = new CommentaryService(db, new RuleBasedCommentaryGenerator());

        var act = async () => await service.GenerateAsync(analysisId, "user1", orgId + 999);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GenerateAsync_CalledTwice_ReplacesPreviousPackRatherThanDuplicating()
    {
        var (db, orgId, analysisId) = await SetupAnalysis();
        var service = new CommentaryService(db, new RuleBasedCommentaryGenerator());

        var first = await service.GenerateAsync(analysisId, "user1", orgId);
        var second = await service.GenerateAsync(analysisId, "user1", orgId);

        (await db.CommentaryPacks.CountAsync()).Should().Be(1);
        (await db.CommentaryPacks.SingleAsync()).Id.Should().Be(second.Id);
        first.Id.Should().NotBe(second.Id);
    }

    [Fact]
    public async Task GenerateAsync_NoOrganizationSettingsConfigured_FallsBackToDefaults()
    {
        var (db, orgId, analysisId) = await SetupAnalysis();
        (await db.OrganizationSettings.AnyAsync(s => s.OrganizationId == orgId)).Should().BeFalse();
        var service = new CommentaryService(db, new RuleBasedCommentaryGenerator());

        // Default MaterialVarianceAmount is 250k; the seeded $500k distribution swing should register as positive.
        var pack = await service.GenerateAsync(analysisId, "user1", orgId);

        pack.PositiveVariances.Should().Contain("Fund A");
    }
}
