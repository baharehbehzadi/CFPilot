using CashflowPilot.Domain.Entities;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;
using Xunit;

namespace CashflowPilot.Tests.Integration;

public class UploadImportIntegrationTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static (ApplicationDbContext db, IUploadService uploadService) SetupServices()
    {
        var db = CreateDb();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Storage:Root"] = Path.GetTempPath() })
            .Build();
        var storage = new FileStorageService(config);
        var parser = new CsvParserService();
        var uploadService = new UploadService(db, storage, parser);
        return (db, uploadService);
    }

    private static IFormFile CreateFormFile(string content, string fileName = "test.csv")
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };
    }

    [Fact]
    public async Task FullUploadImportFlow_ForecastCsv_CreatesRunAndEntries()
    {
        var (db, uploadService) = SetupServices();

        var org = new Organization { Name = "Test Org", CreatedAt = DateTime.UtcNow };
        db.Organizations.Add(org);
        db.SaveChanges();
        var portfolio = new Portfolio { OrganizationId = org.Id, Name = "Test Portfolio", CreatedAt = DateTime.UtcNow };
        db.Portfolios.Add(portfolio);
        db.SaveChanges();

        var csv = "Fund,Period,Capital Calls,Distributions,Currency\n" +
                  "Apex Buyout Fund,2024-01,1500000,0,USD\n" +
                  "Apex Buyout Fund,2024-02,1000000,0,USD\n" +
                  "Nordic Growth Capital,2024-01,750000,200000,EUR\n";

        // Step 1: Save upload
        var fileId = await uploadService.SaveUploadAsync(CreateFormFile(csv), FileType.ForecastCsv, portfolio.Id, "user1", org.Id);
        fileId.Should().BeGreaterThan(0);

        // Step 2: Parse preview
        var parseResult = await uploadService.ParseFileAsync(fileId, null, org.Id);
        parseResult.ValidRows.Should().Be(3);
        parseResult.InvalidRows.Should().Be(0);
        parseResult.GlobalErrors.Should().BeEmpty();

        // Step 3: Import
        var runId = await uploadService.ImportForecastAsync(fileId, "Q1 2024 Forecast", portfolio.Id, "user1", org.Id);
        runId.Should().BeGreaterThan(0);

        var run = await db.ForecastRuns.FindAsync(runId);
        run.Should().NotBeNull();
        run!.Status.Should().Be(RunStatus.Completed);
        run.EntryCount.Should().Be(3);

        var entries = await db.CashflowEntries.Where(e => e.ForecastRunId == runId).ToListAsync();
        entries.Should().HaveCount(3);
        entries.Sum(e => e.CapitalCalls).Should().Be(3_250_000m);

        // Funds auto-created
        var funds = await db.Funds.Where(f => f.PortfolioId == portfolio.Id).ToListAsync();
        funds.Should().HaveCount(2);
        funds.Select(f => f.Name).Should().Contain(new[] { "Apex Buyout Fund", "Nordic Growth Capital" });
    }

    [Fact]
    public async Task ParseFile_InvalidRows_IdentifiesErrors()
    {
        var (db, uploadService) = SetupServices();
        var org = new Organization { Name = "Test Org 2", CreatedAt = DateTime.UtcNow };
        db.Organizations.Add(org);
        db.SaveChanges();
        var portfolio = new Portfolio { OrganizationId = org.Id, Name = "Portfolio", CreatedAt = DateTime.UtcNow };
        db.Portfolios.Add(portfolio);
        db.SaveChanges();

        var csv = "Fund,Period,Capital Calls,Distributions\n" +
                  "Fund A,2024-01,1000000,0\n" +       // valid
                  "Fund B,not-a-date,500000,0\n" +      // invalid period
                  "Fund C,2024-03,not-a-number,0\n";    // invalid amount

        var fileId = await uploadService.SaveUploadAsync(CreateFormFile(csv), FileType.ForecastCsv, portfolio.Id, "user1", org.Id);
        var result = await uploadService.ParseFileAsync(fileId, null, org.Id);

        result.ValidRows.Should().Be(1);
        result.InvalidRows.Should().Be(2);
    }

    [Fact]
    public async Task ImportActual_Flow_CreatesActualRunAndEntries()
    {
        var (db, uploadService) = SetupServices();
        var org = new Organization { Name = "Test Org 3", CreatedAt = DateTime.UtcNow };
        db.Organizations.Add(org);
        db.SaveChanges();
        var portfolio = new Portfolio { OrganizationId = org.Id, Name = "Portfolio", CreatedAt = DateTime.UtcNow };
        db.Portfolios.Add(portfolio);
        db.SaveChanges();

        var csv = "Fund,Period,Capital Calls,Distributions\nApex,2024-01,900000,100000\n";
        var fileId = await uploadService.SaveUploadAsync(CreateFormFile(csv, "actuals.csv"), FileType.ActualCsv, portfolio.Id, "user1", org.Id);

        var runId = await uploadService.ImportActualAsync(fileId, "Jan 2024 Actuals", new DateTime(2024, 1, 1), portfolio.Id, "user1", org.Id);

        var run = await db.ActualRuns.FindAsync(runId);
        run.Should().NotBeNull();
        run!.Status.Should().Be(RunStatus.Completed);
        run.EntryCount.Should().Be(1);

        var entries = await db.CashflowEntries.Where(e => e.ActualRunId == runId).ToListAsync();
        entries[0].CapitalCalls.Should().Be(900_000m);
        entries[0].Distributions.Should().Be(100_000m);
        entries[0].EntryType.Should().Be(EntryType.Actual);
    }

    [Fact]
    public async Task Import_SameFundUploadedTwice_ReusesFundEntity()
    {
        var (db, uploadService) = SetupServices();
        var org = new Organization { Name = "Test Org 4", CreatedAt = DateTime.UtcNow };
        db.Organizations.Add(org);
        db.SaveChanges();
        var portfolio = new Portfolio { OrganizationId = org.Id, Name = "Portfolio", CreatedAt = DateTime.UtcNow };
        db.Portfolios.Add(portfolio);
        db.SaveChanges();

        var csv1 = "Fund,Period,Capital Calls,Distributions\nApex Fund,2024-01,1000000,0\n";
        var csv2 = "Fund,Period,Capital Calls,Distributions\nApex Fund,2024-02,800000,0\n";

        var fileId1 = await uploadService.SaveUploadAsync(CreateFormFile(csv1, "f1.csv"), FileType.ForecastCsv, portfolio.Id, "user1", org.Id);
        await uploadService.ImportForecastAsync(fileId1, "Run 1", portfolio.Id, "user1", org.Id);

        var fileId2 = await uploadService.SaveUploadAsync(CreateFormFile(csv2, "f2.csv"), FileType.ForecastCsv, portfolio.Id, "user1", org.Id);
        await uploadService.ImportForecastAsync(fileId2, "Run 2", portfolio.Id, "user1", org.Id);

        // Should reuse same fund, not create a duplicate
        var funds = await db.Funds.Where(f => f.PortfolioId == portfolio.Id && f.Name == "Apex Fund").ToListAsync();
        funds.Should().HaveCount(1);
    }
}
