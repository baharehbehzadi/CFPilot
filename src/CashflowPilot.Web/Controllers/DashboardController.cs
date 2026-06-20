using CashflowPilot.Domain.Enums;
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

        // KPI 1: Active portfolios
        ViewBag.PortfolioCount = await _db.Portfolios.CountAsync(p => p.OrganizationId == orgId && p.IsActive);

        // KPI 2-4: Inception-to-date actual cashflow (capital called, distributions, net)
        // Materialized client-side because SQLite's EF Core provider cannot translate Sum() over decimal columns.
        var actualEntries = await _db.CashflowEntries
            .Where(e => e.OrganizationId == orgId && e.EntryType == EntryType.Actual)
            .ToListAsync();
        ViewBag.ActualCapitalCalled = actualEntries.Sum(e => e.CapitalCalls);
        ViewBag.ActualDistributions = actualEntries.Sum(e => e.Distributions);
        ViewBag.ActualNetCashflow = ViewBag.ActualDistributions - ViewBag.ActualCapitalCalled;

        // KPI 5-6: Current NAV and unfunded commitment, from each fund's most recent NAV snapshot
        var navSnapshots = await _db.NavSnapshots.Where(n => n.OrganizationId == orgId).ToListAsync();
        var latestNavByFund = navSnapshots.GroupBy(n => n.FundId).Select(g => g.OrderByDescending(n => n.ValuationDate).First()).ToList();
        ViewBag.CurrentNav = latestNavByFund.Sum(n => n.NavAmount);
        ViewBag.UnfundedCommitment = latestNavByFund.Sum(n => n.UnfundedCommitment);
        ViewBag.LatestNavCount = latestNavByFund.Count;

        // KPI 7: Inception-to-date net cashflow variance vs forecast, across all variance analyses
        var varianceEntries = await _db.VarianceAnalyses
            .Where(v => v.OrganizationId == orgId)
            .SelectMany(v => v.Entries)
            .ToListAsync();
        ViewBag.NetVarianceItd = varianceEntries.Sum(e => (e.ActualDistributions - e.ActualCapitalCalls) - (e.ForecastDistributions - e.ForecastCapitalCalls));

        // KPI 8: Open (unresolved, non-informational) validation issues
        ViewBag.OpenValidationIssueCount = await _db.ValidationIssues
            .CountAsync(i => i.OrganizationId == orgId && !i.IsAccepted && i.Severity != ValidationSeverity.Info);

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
