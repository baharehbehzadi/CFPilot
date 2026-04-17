using CashflowPilot.Application.Interfaces;
using CashflowPilot.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Web.Controllers;

[Authorize]
public class CommentaryController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICommentaryService _commentary;
    private readonly IAuditService _audit;

    public CommentaryController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ICommentaryService commentary, IAuditService audit)
    {
        _db = db;
        _userManager = userManager;
        _commentary = commentary;
        _audit = audit;
    }

    private async Task<(ApplicationUser user, int orgId)> GetUserAsync()
    {
        var user = await _userManager.GetUserAsync(User) ?? throw new InvalidOperationException("User not found");
        return (user, user.OrganizationId);
    }

    [Authorize(Policy = "RequireAnalyst")]
    public async Task<IActionResult> Generate(int analysisId)
    {
        var (user, orgId) = await GetUserAsync();
        try
        {
            var pack = await _commentary.GenerateAsync(analysisId, user.Id, orgId);
            await _audit.LogAsync(orgId, user.Id, user.DisplayName, "CommentaryGeneration", "CommentaryPack", pack.Id.ToString(), $"Generated commentary for analysis {analysisId}");
            TempData["Success"] = "Commentary generated.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Commentary generation failed: {ex.Message}";
        }
        return RedirectToAction("Detail", "Analysis", new { id = analysisId });
    }

    public async Task<IActionResult> View(int analysisId)
    {
        var (_, orgId) = await GetUserAsync();
        var pack = await _db.CommentaryPacks
            .Include(c => c.VarianceAnalysis).ThenInclude(v => v.ForecastRun)
            .Include(c => c.VarianceAnalysis).ThenInclude(v => v.ActualRun)
            .FirstOrDefaultAsync(c => c.VarianceAnalysisId == analysisId && c.OrganizationId == orgId);
        if (pack == null) return RedirectToAction("Detail", "Analysis", new { id = analysisId });
        return View(pack);
    }

    public async Task<IActionResult> PrintReport(int analysisId)
    {
        var (_, orgId) = await GetUserAsync();
        var pack = await _db.CommentaryPacks
            .Include(c => c.VarianceAnalysis).ThenInclude(v => v.ForecastRun)
            .Include(c => c.VarianceAnalysis).ThenInclude(v => v.ActualRun)
            .FirstOrDefaultAsync(c => c.VarianceAnalysisId == analysisId && c.OrganizationId == orgId);
        if (pack == null) return NotFound();
        // Render print-friendly view with no layout
        return View(pack);
    }
}
