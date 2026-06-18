using CashflowPilot.Application.DTOs;
using CashflowPilot.Domain.Entities;

namespace CashflowPilot.Application.Interfaces;

public interface INavSnapshotService
{
    Task<int> CreateAsync(NavSnapshotCreateDto dto, string userId, int organizationId);
    Task<List<NavSnapshot>> GetByOrganizationAsync(int organizationId);
    Task<NavSnapshot?> GetByIdAsync(int id, int organizationId);
    Task DeleteAsync(int id, int organizationId);
}
