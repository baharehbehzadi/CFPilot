using CashflowPilot.Application.DTOs;
using CashflowPilot.Application.Interfaces;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Web.Controllers;

[Authorize]
public class NavSnapshotController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INavSnapshotService _navSnapshots;
    private readonly IAuditService _audit;

    public NavSnapshotController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, INavSnapshotService navSnapshots, IAuditService audit)
    {
        _db = db;
        _userManager = userManager;
        _navSnapshots = navSnapshots;
        _audit = audit;
    }

    private async Task<(ApplicationUser user, int orgId)> GetUserAsync()
    {
        var user = await _userManager.GetUserAsync(User) ?? throw new InvalidOperationException("User not found");
        return (user, user.OrganizationId);
    }

    public async Task<IActionResult> Index()
    {
        var (_, orgId) = await GetUserAsync();
        var snapshots = await _navSnapshots.GetByOrganizationAsync(orgId);
        return View(new NavSnapshotListViewModel { Snapshots = snapshots });
    }

    [Authorize(Policy = "RequireAnalyst")]
    public async Task<IActionResult> Create()
    {
        var (_, orgId) = await GetUserAsync();
        return View(await BuildCreateViewModel(orgId));
    }

    [HttpPost, Authorize(Policy = "RequireAnalyst"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateNavSnapshotViewModel model)
    {
        var (user, orgId) = await GetUserAsync();
        if (!ModelState.IsValid)
        {
            var vm = await BuildCreateViewModel(orgId);
            vm.PortfolioId = model.PortfolioId; vm.FundId = model.FundId; vm.ReportingPeriodId = model.ReportingPeriodId;
            vm.ValuationDate = model.ValuationDate; vm.NavAmount = model.NavAmount; vm.UnfundedCommitment = model.UnfundedCommitment;
            vm.PaidInCapital = model.PaidInCapital; vm.TotalDistributions = model.TotalDistributions; vm.Currency = model.Currency;
            return View(vm);
        }

        var id = await _navSnapshots.CreateAsync(new NavSnapshotCreateDto
        {
            PortfolioId = model.PortfolioId, FundId = model.FundId, ReportingPeriodId = model.ReportingPeriodId,
            ValuationDate = model.ValuationDate, NavAmount = model.NavAmount, UnfundedCommitment = model.UnfundedCommitment,
            PaidInCapital = model.PaidInCapital, TotalDistributions = model.TotalDistributions, Currency = model.Currency
        }, user.Id, orgId);

        await _audit.LogAsync(orgId, user.Id, user.DisplayName, "NavSnapshotCreated", "NavSnapshot", id.ToString(), $"Recorded NAV snapshot as of {model.ValuationDate:yyyy-MM-dd}");
        return RedirectToAction("Index");
    }

    [HttpPost, Authorize(Policy = "RequireAnalyst"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (user, orgId) = await GetUserAsync();
        await _navSnapshots.DeleteAsync(id, orgId);
        await _audit.LogAsync(orgId, user.Id, user.DisplayName, "NavSnapshotDeleted", "NavSnapshot", id.ToString(), "Deleted NAV snapshot");
        return RedirectToAction("Index");
    }

    private async Task<CreateNavSnapshotViewModel> BuildCreateViewModel(int orgId)
    {
        return new CreateNavSnapshotViewModel
        {
            Portfolios = await _db.Portfolios.Where(p => p.OrganizationId == orgId && p.IsActive).ToListAsync(),
            Funds = await _db.Funds.Where(f => f.OrganizationId == orgId && f.IsActive).ToListAsync(),
            ReportingPeriods = await _db.ReportingPeriods.Where(r => r.OrganizationId == orgId).OrderByDescending(r => r.PeriodStartDate).ToListAsync()
        };
    }
}
