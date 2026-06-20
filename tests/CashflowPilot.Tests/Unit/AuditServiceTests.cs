using CashflowPilot.Domain.Entities;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CashflowPilot.Tests.Unit;

public class AuditServiceTests
{
    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task LogAsync_WithoutOldOrNewValue_LeavesJsonColumnsNull()
    {
        var db = CreateDb();
        var service = new AuditService(db);

        await service.LogAsync(1, "user1", "Jane Analyst", "Login", "User", "user1", "User logged in");

        var log = await db.AuditLogs.SingleAsync();
        log.OldValueJson.Should().BeNull();
        log.NewValueJson.Should().BeNull();
    }

    [Fact]
    public async Task LogAsync_WithOldAndNewValue_SerializesBothAsJson()
    {
        var db = CreateDb();
        var service = new AuditService(db);

        await service.LogAsync(1, "user1", "Jane Analyst", "ValidationIssueAccepted", "ValidationIssue", "5",
            "Currency mismatch", oldValue: new { IsAccepted = false }, newValue: new { IsAccepted = true });

        var log = await db.AuditLogs.SingleAsync();
        log.OldValueJson.Should().Be("{\"IsAccepted\":false}");
        log.NewValueJson.Should().Be("{\"IsAccepted\":true}");
    }

    [Fact]
    public async Task LogAsync_WithNewValueOnly_LeavesOldValueJsonNull()
    {
        var db = CreateDb();
        var service = new AuditService(db);

        await service.LogAsync(1, "user1", "Jane Analyst", "NavSnapshotCreated", "NavSnapshot", "10",
            "Recorded NAV snapshot", newValue: new { NavAmount = 1_000_000m });

        var log = await db.AuditLogs.SingleAsync();
        log.OldValueJson.Should().BeNull();
        log.NewValueJson.Should().Contain("1000000");
    }
}
