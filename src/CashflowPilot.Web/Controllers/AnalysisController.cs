using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Services;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Web.Controllers;

[Authorize]
public class AnalysisController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IVarianceService _variance;
    private readonly IReportExportService _export;
    private readonly IAuditService _audit;

    public AnalysisController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IVarianceService variance, IReportExportService export, IAuditService audit)
    {
        _db = db;
        _userManager = userManager;
        _variance = variance;
        _export = export;
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
        var analyses = await _db.VarianceAnalyses
            .Where(v => v.OrganizationId == orgId)
            .Include(v => v.ForecastRun).Include(v => v.ActualRun)
            .Include(v => v.CommentaryPack)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();
        return View(analyses);
    }

    [Authorize(Policy = "RequireAnalyst")]
    public async Task<IActionResult> Create()
    {
        var (_, orgId) = await GetUserAsync();
        return View(new CreateAnalysisViewModel
        {
            ForecastRuns = await _db.ForecastRuns.Where(r => r.OrganizationId == orgId && r.Status == RunStatus.Completed).Include(r => r.Portfolio).OrderByDescending(r => r.RunDate).ToListAsync(),
            ActualRuns = await _db.ActualRuns.Where(r => r.OrganizationId == orgId && r.Status == RunStatus.Completed).Include(r => r.Portfolio).OrderByDescending(r => r.RunDate).ToListAsync()
        });
    }

    [HttpPost, Authorize(Policy = "RequireAnalyst"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAnalysisViewModel model)
    {
        var (user, orgId) = await GetUserAsync();
        if (!ModelState.IsValid)
        {
            model.ForecastRuns = await _db.ForecastRuns.Where(r => r.OrganizationId == orgId && r.Status == RunStatus.Completed).Include(r => r.Portfolio).OrderByDescending(r => r.RunDate).ToListAsync();
            model.ActualRuns = await _db.ActualRuns.Where(r => r.OrganizationId == orgId && r.Status == RunStatus.Completed).Include(r => r.Portfolio).OrderByDescending(r => r.RunDate).ToListAsync();
            return View(model);
        }
        try
        {
            var id = await _variance.CreateAnalysisAsync(model.ForecastRunId, model.ActualRunId, user.Id, orgId, model.Name);
            await _audit.LogAsync(orgId, user.Id, user.DisplayName, "AnalysisRun", "VarianceAnalysis", id.ToString(), $"Created variance analysis: {model.Name}");
            TempData["Success"] = "Variance analysis created.";
            return RedirectToAction("Detail", new { id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.ForecastRuns = await _db.ForecastRuns.Where(r => r.OrganizationId == orgId && r.Status == RunStatus.Completed).Include(r => r.Portfolio).OrderByDescending(r => r.RunDate).ToListAsync();
            model.ActualRuns = await _db.ActualRuns.Where(r => r.OrganizationId == orgId && r.Status == RunStatus.Completed).Include(r => r.Portfolio).OrderByDescending(r => r.RunDate).ToListAsync();
            return View(model);
        }
    }

    public async Task<IActionResult> Detail(int id)
    {
        var (_, orgId) = await GetUserAsync();
        var summary = await _variance.GetSummaryAsync(id, orgId);
        if (summary == null) return NotFound();
        var commentary = await _db.CommentaryPacks.FirstOrDefaultAsync(c => c.VarianceAnalysisId == id && c.OrganizationId == orgId);
        return View(new AnalysisDetailViewModel { Summary = summary, Commentary = commentary });
    }

    public async Task<IActionResult> ExportCsv(int id)
    {
        var (user, orgId) = await GetUserAsync();
        var summary = await _variance.GetSummaryAsync(id, orgId);
        if (summary == null) return NotFound();
        await _audit.LogAsync(orgId, user.Id, user.DisplayName, "Export", "VarianceAnalysis", id.ToString(), "Exported CSV");
        var bytes = _export.ExportVarianceToCsv(summary);
        return File(bytes, "text/csv", $"variance-analysis-{id}-{DateTime.UtcNow:yyyyMMdd}.csv");
    }

    public async Task<IActionResult> ExportExcel(int id)
    {
        var (user, orgId) = await GetUserAsync();
        var summary = await _variance.GetSummaryAsync(id, orgId);
        if (summary == null) return NotFound();
        await _audit.LogAsync(orgId, user.Id, user.DisplayName, "Export", "VarianceAnalysis", id.ToString(), "Exported Excel");
        var bytes = _export.ExportVarianceToExcel(summary);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"variance-analysis-{id}-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }

    [HttpPost, Authorize(Policy = "RequireAnalyst"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (user, orgId) = await GetUserAsync();
        var analysis = await _db.VarianceAnalyses.FirstOrDefaultAsync(v => v.Id == id && v.OrganizationId == orgId);
        if (analysis != null)
        {
            _db.VarianceAnalyses.Remove(analysis);
            await _db.SaveChangesAsync();
            await _audit.LogAsync(orgId, user.Id, user.DisplayName, "Delete", "VarianceAnalysis", id.ToString(), "Deleted variance analysis");
        }
        return RedirectToAction("Index");
    }
}
