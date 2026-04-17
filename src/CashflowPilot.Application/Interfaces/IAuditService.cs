namespace CashflowPilot.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(int organizationId, string userId, string userName, string action, string? entityType = null, string? entityId = null, string? details = null, string? ipAddress = null);
}
