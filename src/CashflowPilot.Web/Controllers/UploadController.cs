using CashflowPilot.Application.DTOs;
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

[Authorize(Policy = "RequireAnalyst")]
public class UploadController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUploadService _uploadService;
    private readonly IAuditService _audit;

    public UploadController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IUploadService uploadService, IAuditService audit)
    {
        _db = db;
        _userManager = userManager;
        _uploadService = uploadService;
        _audit = audit;
    }

    private async Task<(ApplicationUser user, int orgId)> GetUserAsync()
    {
        var user = await _userManager.GetUserAsync(User) ?? throw new InvalidOperationException("User not found");
        return (user, user.OrganizationId);
    }

    [HttpGet]
    public async Task<IActionResult> UploadForecast()
    {
        var (_, orgId) = await GetUserAsync();
        var vm = new UploadForecastViewModel
        {
            Portfolios = await _db.Portfolios.Where(p => p.OrganizationId == orgId && p.IsActive).ToListAsync()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadForecast(UploadForecastViewModel model)
    {
        var (user, orgId) = await GetUserAsync();
        model.Portfolios = await _db.Portfolios.Where(p => p.OrganizationId == orgId && p.IsActive).ToListAsync();
        if (!ModelState.IsValid) return View(model);
        if (model.File == null || model.File.Length == 0) { ModelState.AddModelError("File", "Please select a file."); return View(model); }

        try
        {
            var fileId = await _uploadService.SaveUploadAsync(model.File, FileType.ForecastCsv, model.PortfolioId, user.Id, orgId);
            var parseResult = await _uploadService.ParseFileAsync(fileId, null, orgId);
            await _audit.LogAsync(orgId, user.Id, user.DisplayName, "Upload", "UploadedFile", fileId.ToString(), $"Uploaded forecast CSV: {model.File.FileName}", HttpContext.Connection.RemoteIpAddress?.ToString());

            if (parseResult.Mapping == null || !parseResult.Mapping.IsComplete())
            {
                var mv = new MappingViewModel
                {
                    UploadedFileId = fileId, FileType = "Forecast", PortfolioId = model.PortfolioId,
                    RunName = model.RunName, DetectedHeaders = parseResult.DetectedHeaders,
                    FundNameColumn = parseResult.Mapping?.FundNameColumn, StrategyColumn = parseResult.Mapping?.StrategyColumn,
                    PeriodColumn = parseResult.Mapping?.PeriodColumn, CapitalCallsColumn = parseResult.Mapping?.CapitalCallsColumn,
                    DistributionsColumn = parseResult.Mapping?.DistributionsColumn
                };
                return View("Mapping", mv);
            }

            return View("ValidationResult", new ValidationResultViewModel
            {
                UploadedFileId = fileId, FileName = model.File.FileName, ParseResult = parseResult,
                PortfolioId = model.PortfolioId, RunName = model.RunName, FileType = "Forecast"
            });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Upload failed: {ex.Message}");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> UploadActual()
    {
        var (_, orgId) = await GetUserAsync();
        var vm = new UploadActualViewModel
        {
            Portfolios = await _db.Portfolios.Where(p => p.OrganizationId == orgId && p.IsActive).ToListAsync()
        };
        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadActual(UploadActualViewModel model)
    {
        var (user, orgId) = await GetUserAsync();
        model.Portfolios = await _db.Portfolios.Where(p => p.OrganizationId == orgId && p.IsActive).ToListAsync();
        if (!ModelState.IsValid) return View(model);
        if (model.File == null || model.File.Length == 0) { ModelState.AddModelError("File", "Please select a file."); return View(model); }

        try
        {
            var fileId = await _uploadService.SaveUploadAsync(model.File, FileType.ActualCsv, model.PortfolioId, user.Id, orgId);
            var parseResult = await _uploadService.ParseFileAsync(fileId, null, orgId);
            await _audit.LogAsync(orgId, user.Id, user.DisplayName, "Upload", "UploadedFile", fileId.ToString(), $"Uploaded actual CSV: {model.File.FileName}", HttpContext.Connection.RemoteIpAddress?.ToString());

            if (parseResult.Mapping == null || !parseResult.Mapping.IsComplete())
            {
                var mv = new MappingViewModel
                {
                    UploadedFileId = fileId, FileType = "Actual", PortfolioId = model.PortfolioId,
                    RunName = model.RunName, ReportingPeriod = model.ReportingPeriod,
                    DetectedHeaders = parseResult.DetectedHeaders
                };
                return View("Mapping", mv);
            }

            return View("ValidationResult", new ValidationResultViewModel
            {
                UploadedFileId = fileId, FileName = model.File.FileName, ParseResult = parseResult,
                PortfolioId = model.PortfolioId, RunName = model.RunName, FileType = "Actual",
                ReportingPeriod = model.ReportingPeriod
            });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Upload failed: {ex.Message}");
            return View(model);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyMapping(MappingViewModel model)
    {
        var (user, orgId) = await GetUserAsync();
        var mapping = new ColumnMapping
        {
            FundNameColumn = model.FundNameColumn, StrategyColumn = model.StrategyColumn,
            PeriodColumn = model.PeriodColumn, CapitalCallsColumn = model.CapitalCallsColumn,
            DistributionsColumn = model.DistributionsColumn, CurrencyColumn = model.CurrencyColumn,
            NotesColumn = model.NotesColumn
        };
        var parseResult = await _uploadService.ParseFileAsync(model.UploadedFileId, mapping, orgId);
        var file = await _db.UploadedFiles.FindAsync(model.UploadedFileId);
        return View("ValidationResult", new ValidationResultViewModel
        {
            UploadedFileId = model.UploadedFileId, FileName = file?.OriginalFileName ?? "", ParseResult = parseResult,
            PortfolioId = model.PortfolioId, RunName = model.RunName, FileType = model.FileType,
            ReportingPeriod = model.ReportingPeriod
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(ValidationResultViewModel model)
    {
        var (user, orgId) = await GetUserAsync();
        try
        {
            int runId;
            if (model.FileType == "Forecast")
            {
                runId = await _uploadService.ImportForecastAsync(model.UploadedFileId, model.RunName, model.PortfolioId, user.Id, orgId);
                await _audit.LogAsync(orgId, user.Id, user.DisplayName, "Import", "ForecastRun", runId.ToString(), $"Imported forecast run: {model.RunName}");
            }
            else
            {
                runId = await _uploadService.ImportActualAsync(model.UploadedFileId, model.RunName, model.ReportingPeriod ?? DateTime.UtcNow, model.PortfolioId, user.Id, orgId);
                await _audit.LogAsync(orgId, user.Id, user.DisplayName, "Import", "ActualRun", runId.ToString(), $"Imported actual run: {model.RunName}");
            }
            TempData["Success"] = $"{model.FileType} run '{model.RunName}' imported successfully ({runId}).";
            return RedirectToAction("Index", "Analysis");
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Import failed: {ex.Message}";
            return RedirectToAction(model.FileType == "Forecast" ? "UploadForecast" : "UploadActual");
        }
    }
}
