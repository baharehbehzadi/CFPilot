using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Entities;
using CashflowPilot.Infrastructure.Data;

namespace CashflowPilot.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;

    public AuditService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(int organizationId, string userId, string userName, string action,
        string? entityType = null, string? entityId = null, string? details = null, string? ipAddress = null,
        object? oldValue = null, object? newValue = null)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            OrganizationId = organizationId,
            UserId = userId,
            UserName = userName,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Details = details,
            Timestamp = DateTime.UtcNow,
            IpAddress = ipAddress,
            OldValueJson = oldValue != null ? System.Text.Json.JsonSerializer.Serialize(oldValue) : null,
            NewValueJson = newValue != null ? System.Text.Json.JsonSerializer.Serialize(newValue) : null
        });
        await _db.SaveChangesAsync();
    }
}
