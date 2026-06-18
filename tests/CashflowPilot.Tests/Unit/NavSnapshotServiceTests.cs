using CashflowPilot.Application.DTOs;
using CashflowPilot.Domain.Entities;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CashflowPilot.Tests.Unit;

public class NavSnapshotServiceTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static (ApplicationDbContext db, int orgId, int portfolioId, int fundId) SetupOrgWithFund()
    {
        var db = CreateDb();
        var org = new Organization { Name = "Test Org", CreatedAt = DateTime.UtcNow };
        db.Organizations.Add(org);
        db.SaveChanges();

        var portfolio = new Portfolio { OrganizationId = org.Id, Name = "Test Portfolio", CreatedAt = DateTime.UtcNow };
        db.Portfolios.Add(portfolio);
        db.SaveChanges();

        var fund = new Fund { OrganizationId = org.Id, PortfolioId = portfolio.Id, Name = "Apex Fund", CreatedAt = DateTime.UtcNow };
        db.Funds.Add(fund);
        db.SaveChanges();

        return (db, org.Id, portfolio.Id, fund.Id);
    }

    [Fact]
    public async Task CreateAsync_PersistsSnapshot()
    {
        var (db, orgId, portfolioId, fundId) = SetupOrgWithFund();
        var service = new NavSnapshotService(db);

        var dto = new NavSnapshotCreateDto
        {
            PortfolioId = portfolioId,
            FundId = fundId,
            ValuationDate = new DateTime(2024, 3, 31),
            NavAmount = 5_000_000,
            UnfundedCommitment = 2_000_000,
            PaidInCapital = 3_000_000,
            TotalDistributions = 1_000_000,
            Currency = "USD"
        };

        var id = await service.CreateAsync(dto, "user1", orgId);

        var saved = await db.NavSnapshots.FindAsync(id);
        saved.Should().NotBeNull();
        saved!.OrganizationId.Should().Be(orgId);
        saved.NavAmount.Should().Be(5_000_000);
        saved.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task GetByOrganizationAsync_OrdersByValuationDateDescending_AndIncludesNavigations()
    {
        var (db, orgId, portfolioId, fundId) = SetupOrgWithFund();
        var service = new NavSnapshotService(db);

        await service.CreateAsync(new NavSnapshotCreateDto
        {
            PortfolioId = portfolioId, FundId = fundId, ValuationDate = new DateTime(2024, 1, 31),
            NavAmount = 1_000_000, UnfundedCommitment = 0, PaidInCapital = 0, TotalDistributions = 0
        }, "user1", orgId);
        await service.CreateAsync(new NavSnapshotCreateDto
        {
            PortfolioId = portfolioId, FundId = fundId, ValuationDate = new DateTime(2024, 3, 31),
            NavAmount = 2_000_000, UnfundedCommitment = 0, PaidInCapital = 0, TotalDistributions = 0
        }, "user1", orgId);

        var results = await service.GetByOrganizationAsync(orgId);

        results.Should().HaveCount(2);
        results[0].ValuationDate.Should().Be(new DateTime(2024, 3, 31));
        results[1].ValuationDate.Should().Be(new DateTime(2024, 1, 31));
        results[0].Fund.Should().NotBeNull();
        results[0].Portfolio.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WrongOrganization_ReturnsNull()
    {
        var (db, orgId, portfolioId, fundId) = SetupOrgWithFund();
        var service = new NavSnapshotService(db);

        var id = await service.CreateAsync(new NavSnapshotCreateDto
        {
            PortfolioId = portfolioId, FundId = fundId, ValuationDate = new DateTime(2024, 1, 31),
            NavAmount = 1_000_000, UnfundedCommitment = 0, PaidInCapital = 0, TotalDistributions = 0
        }, "user1", orgId);

        var result = await service.GetByIdAsync(id, orgId + 999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_RemovesSnapshot()
    {
        var (db, orgId, portfolioId, fundId) = SetupOrgWithFund();
        var service = new NavSnapshotService(db);

        var id = await service.CreateAsync(new NavSnapshotCreateDto
        {
            PortfolioId = portfolioId, FundId = fundId, ValuationDate = new DateTime(2024, 1, 31),
            NavAmount = 1_000_000, UnfundedCommitment = 0, PaidInCapital = 0, TotalDistributions = 0
        }, "user1", orgId);

        await service.DeleteAsync(id, orgId);

        var result = await service.GetByIdAsync(id, orgId);
        result.Should().BeNull();
    }
}
