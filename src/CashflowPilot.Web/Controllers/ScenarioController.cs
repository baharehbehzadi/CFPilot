using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Entities;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Web.Controllers;

[Authorize]
public class ScenarioController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IScenarioService _scenario;
    private readonly IAuditService _audit;

    public ScenarioController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IScenarioService scenario, IAuditService audit)
    {
        _db = db;
        _userManager = userManager;
        _scenario = scenario;
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
        var scenarios = await _db.Scenarios
            .Where(s => s.OrganizationId == orgId)
            .Include(s => s.ForecastRun)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
        return View(new ScenarioListViewModel { Scenarios = scenarios });
    }

    [Authorize(Policy = "RequireAnalyst")]
    public async Task<IActionResult> Create()
    {
        var (_, orgId) = await GetUserAsync();
        return View(new CreateScenarioViewModel
        {
            ForecastRuns = await _db.ForecastRuns.Where(r => r.OrganizationId == orgId && r.Status == RunStatus.Completed).Include(r => r.Portfolio).OrderByDescending(r => r.RunDate).ToListAsync()
        });
    }

    [HttpPost, Authorize(Policy = "RequireAnalyst"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateScenarioViewModel model)
    {
        var (user, orgId) = await GetUserAsync();
        if (!ModelState.IsValid)
        {
            model.ForecastRuns = await _db.ForecastRuns.Where(r => r.OrganizationId == orgId && r.Status == RunStatus.Completed).Include(r => r.Portfolio).OrderByDescending(r => r.RunDate).ToListAsync();
            return View(model);
        }
        var scenario = new Scenario
        {
            OrganizationId = orgId,
            ForecastRunId = model.ForecastRunId,
            Name = model.Name,
            Description = model.Description,
            CallsAdjustmentPct = model.CallsAdjustmentPct,
            DistributionsAdjustmentPct = model.DistributionsAdjustmentPct,
            TimingShiftMonths = model.TimingShiftMonths,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = user.Id
        };
        _db.Scenarios.Add(scenario);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(orgId, user.Id, user.DisplayName, "ScenarioCreation", "Scenario", scenario.Id.ToString(), $"Created scenario: {model.Name}");
        return RedirectToAction("Detail", new { id = scenario.Id });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var (_, orgId) = await GetUserAsync();
        var scenario = await _db.Scenarios.Include(s => s.ForecastRun).FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == orgId);
        if (scenario == null) return NotFound();
        var result = await _scenario.ComputeScenarioAsync(id, orgId);
        return View(new ScenarioDetailViewModel { Scenario = scenario, Result = result });
    }

    [HttpPost, Authorize(Policy = "RequireAnalyst"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (user, orgId) = await GetUserAsync();
        var scenario = await _db.Scenarios.FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == orgId);
        if (scenario != null)
        {
            _db.Scenarios.Remove(scenario);
            await _db.SaveChangesAsync();
            await _audit.LogAsync(orgId, user.Id, user.DisplayName, "ScenarioDelete", "Scenario", id.ToString(), $"Deleted scenario: {scenario.Name}");
        }
        return RedirectToAction("Index");
    }
}
