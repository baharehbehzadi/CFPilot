using CashflowPilot.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();
        int orgId = user.OrganizationId;

        ViewBag.PortfolioCount = await _db.Portfolios.CountAsync(p => p.OrganizationId == orgId && p.IsActive);
        ViewBag.ForecastRunCount = await _db.ForecastRuns.CountAsync(r => r.OrganizationId == orgId);
        ViewBag.ActualRunCount = await _db.ActualRuns.CountAsync(r => r.OrganizationId == orgId);
        ViewBag.AnalysisCount = await _db.VarianceAnalyses.CountAsync(v => v.OrganizationId == orgId);
        ViewBag.ScenarioCount = await _db.Scenarios.CountAsync(s => s.OrganizationId == orgId);

        ViewBag.RecentAuditLogs = await _db.AuditLogs
            .Where(a => a.OrganizationId == orgId)
            .OrderByDescending(a => a.Timestamp)
            .Take(5)
            .ToListAsync();

        ViewBag.RecentForecastRuns = await _db.ForecastRuns
            .Where(r => r.OrganizationId == orgId)
            .Include(r => r.Portfolio)
            .OrderByDescending(r => r.RunDate)
            .Take(3)
            .ToListAsync();

        ViewBag.RecentActualRuns = await _db.ActualRuns
            .Where(r => r.OrganizationId == orgId)
            .Include(r => r.Portfolio)
            .OrderByDescending(r => r.RunDate)
            .Take(3)
            .ToListAsync();

        ViewBag.RecentAnalyses = await _db.VarianceAnalyses
            .Where(v => v.OrganizationId == orgId)
            .Include(v => v.ForecastRun)
            .Include(v => v.ActualRun)
            .OrderByDescending(v => v.CreatedAt)
            .Take(3)
            .ToListAsync();

        return View();
    }
}
