using CashflowPilot.Application.DTOs;
using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Entities;
using CashflowPilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Infrastructure.Services;

public class NavSnapshotService : INavSnapshotService
{
    private readonly ApplicationDbContext _db;

    public NavSnapshotService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<int> CreateAsync(NavSnapshotCreateDto dto, string userId, int organizationId)
    {
        var snapshot = new NavSnapshot
        {
            OrganizationId = organizationId,
            PortfolioId = dto.PortfolioId,
            FundId = dto.FundId,
            ReportingPeriodId = dto.ReportingPeriodId,
            ValuationDate = dto.ValuationDate,
            NavAmount = dto.NavAmount,
            UnfundedCommitment = dto.UnfundedCommitment,
            PaidInCapital = dto.PaidInCapital,
            TotalDistributions = dto.TotalDistributions,
            Currency = dto.Currency
        };
        _db.NavSnapshots.Add(snapshot);
        await _db.SaveChangesAsync();
        return snapshot.Id;
    }

    public async Task<List<NavSnapshot>> GetByOrganizationAsync(int organizationId)
    {
        return await _db.NavSnapshots
            .Where(n => n.OrganizationId == organizationId)
            .Include(n => n.Fund)
            .Include(n => n.Portfolio)
            .OrderByDescending(n => n.ValuationDate)
            .ToListAsync();
    }

    public async Task<NavSnapshot?> GetByIdAsync(int id, int organizationId)
    {
        return await _db.NavSnapshots
            .Include(n => n.Fund)
            .Include(n => n.Portfolio)
            .FirstOrDefaultAsync(n => n.Id == id && n.OrganizationId == organizationId);
    }

    public async Task DeleteAsync(int id, int organizationId)
    {
        var snapshot = await _db.NavSnapshots.FirstOrDefaultAsync(n => n.Id == id && n.OrganizationId == organizationId);
        if (snapshot != null)
        {
            _db.NavSnapshots.Remove(snapshot);
            await _db.SaveChangesAsync();
        }
    }
}
