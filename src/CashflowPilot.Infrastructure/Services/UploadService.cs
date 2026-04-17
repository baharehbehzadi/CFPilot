using CashflowPilot.Application.DTOs;
using CashflowPilot.Domain.Entities;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Infrastructure.Services;

public interface IUploadService
{
    Task<int> SaveUploadAsync(IFormFile file, FileType fileType, int portfolioId, string userId, int organizationId);
    Task<ParseResultDto> ParseFileAsync(int uploadedFileId, ColumnMapping? mapping, int organizationId);
    Task<int> ImportForecastAsync(int uploadedFileId, string runName, int portfolioId, string userId, int organizationId, ColumnMapping? mapping = null);
    Task<int> ImportActualAsync(int uploadedFileId, string runName, DateTime reportingPeriod, int portfolioId, string userId, int organizationId, ColumnMapping? mapping = null);
}

public class UploadService : IUploadService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorageService _storage;
    private readonly ICsvParserService _parser;

    public UploadService(ApplicationDbContext db, IFileStorageService storage, ICsvParserService parser)
    {
        _db = db;
        _storage = storage;
        _parser = parser;
    }

    public async Task<int> SaveUploadAsync(IFormFile file, FileType fileType, int portfolioId, string userId, int organizationId)
    {
        var (storedName, fullPath) = await _storage.SaveUploadAsync(file, $"uploads/{organizationId}");
        var entity = new UploadedFile
        {
            OrganizationId = organizationId,
            OriginalFileName = file.FileName,
            StoredFileName = storedName,
            FilePath = fullPath,
            FileSize = file.Length,
            ContentType = file.ContentType,
            UploadedAt = DateTime.UtcNow,
            UploadedByUserId = userId,
            FileType = fileType,
            Status = FileStatus.Uploaded
        };
        _db.UploadedFiles.Add(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<ParseResultDto> ParseFileAsync(int uploadedFileId, ColumnMapping? mapping, int organizationId)
    {
        var file = await _db.UploadedFiles.FirstOrDefaultAsync(f => f.Id == uploadedFileId && f.OrganizationId == organizationId)
            ?? throw new InvalidOperationException($"Uploaded file {uploadedFileId} not found.");
        using var stream = _storage.OpenRead(file.FilePath);
        return _parser.Parse(stream, mapping);
    }

    public async Task<int> ImportForecastAsync(int uploadedFileId, string runName, int portfolioId, string userId, int organizationId, ColumnMapping? mapping = null)
    {
        var parseResult = await ParseFileAsync(uploadedFileId, mapping, organizationId);
        if (!parseResult.Success && parseResult.ValidRows == 0)
            throw new InvalidOperationException("No valid rows to import. " + string.Join(" ", parseResult.GlobalErrors));

        var run = new ForecastRun
        {
            OrganizationId = organizationId,
            PortfolioId = portfolioId,
            Name = runName,
            RunDate = DateTime.UtcNow,
            CreatedByUserId = userId,
            UploadedFileId = uploadedFileId,
            Status = RunStatus.Processing
        };
        _db.ForecastRuns.Add(run);
        await _db.SaveChangesAsync();

        var fundCache = new Dictionary<string, Fund>();
        var strategyCache = new Dictionary<string, Strategy>();
        int count = 0;

        foreach (var row in parseResult.Rows.Where(r => r.IsValid))
        {
            var fund = await GetOrCreateFundAsync(row.FundName, portfolioId, organizationId, fundCache);
            var strategy = row.StrategyName != null ? await GetOrCreateStrategyAsync(row.StrategyName, fund.Id, organizationId, strategyCache) : null;

            _db.CashflowEntries.Add(new CashflowEntry
            {
                OrganizationId = organizationId,
                ForecastRunId = run.Id,
                PortfolioId = portfolioId,
                FundId = fund.Id,
                FundName = fund.Name,
                StrategyId = strategy?.Id,
                StrategyName = strategy?.Name,
                Period = row.PeriodDate!.Value,
                EntryType = EntryType.Forecast,
                CapitalCalls = row.CapitalCallsAmount ?? 0,
                Distributions = row.DistributionsAmount ?? 0,
                Currency = string.IsNullOrWhiteSpace(row.Currency) ? "USD" : row.Currency,
                Notes = row.Notes
            });
            count++;

            if (count % 200 == 0) await _db.SaveChangesAsync();
        }

        run.Status = RunStatus.Completed;
        run.EntryCount = count;
        await _db.SaveChangesAsync();

        var uploadedFile = await _db.UploadedFiles.FindAsync(uploadedFileId);
        if (uploadedFile != null) { uploadedFile.Status = FileStatus.Processed; await _db.SaveChangesAsync(); }

        return run.Id;
    }

    public async Task<int> ImportActualAsync(int uploadedFileId, string runName, DateTime reportingPeriod, int portfolioId, string userId, int organizationId, ColumnMapping? mapping = null)
    {
        var parseResult = await ParseFileAsync(uploadedFileId, mapping, organizationId);
        if (!parseResult.Success && parseResult.ValidRows == 0)
            throw new InvalidOperationException("No valid rows to import. " + string.Join(" ", parseResult.GlobalErrors));

        var run = new ActualRun
        {
            OrganizationId = organizationId,
            PortfolioId = portfolioId,
            Name = runName,
            ReportingPeriod = reportingPeriod,
            RunDate = DateTime.UtcNow,
            CreatedByUserId = userId,
            UploadedFileId = uploadedFileId,
            Status = RunStatus.Processing
        };
        _db.ActualRuns.Add(run);
        await _db.SaveChangesAsync();

        var fundCache = new Dictionary<string, Fund>();
        var strategyCache = new Dictionary<string, Strategy>();
        int count = 0;

        foreach (var row in parseResult.Rows.Where(r => r.IsValid))
        {
            var fund = await GetOrCreateFundAsync(row.FundName, portfolioId, organizationId, fundCache);
            var strategy = row.StrategyName != null ? await GetOrCreateStrategyAsync(row.StrategyName, fund.Id, organizationId, strategyCache) : null;

            _db.CashflowEntries.Add(new CashflowEntry
            {
                OrganizationId = organizationId,
                ActualRunId = run.Id,
                PortfolioId = portfolioId,
                FundId = fund.Id,
                FundName = fund.Name,
                StrategyId = strategy?.Id,
                StrategyName = strategy?.Name,
                Period = row.PeriodDate!.Value,
                EntryType = EntryType.Actual,
                CapitalCalls = row.CapitalCallsAmount ?? 0,
                Distributions = row.DistributionsAmount ?? 0,
                Currency = string.IsNullOrWhiteSpace(row.Currency) ? "USD" : row.Currency,
                Notes = row.Notes
            });
            count++;

            if (count % 200 == 0) await _db.SaveChangesAsync();
        }

        run.Status = RunStatus.Completed;
        run.EntryCount = count;
        await _db.SaveChangesAsync();

        var uploadedFile = await _db.UploadedFiles.FindAsync(uploadedFileId);
        if (uploadedFile != null) { uploadedFile.Status = FileStatus.Processed; await _db.SaveChangesAsync(); }

        return run.Id;
    }

    private async Task<Fund> GetOrCreateFundAsync(string? fundName, int portfolioId, int organizationId, Dictionary<string, Fund> cache)
    {
        var name = string.IsNullOrWhiteSpace(fundName) ? "Unknown Fund" : fundName.Trim();
        if (cache.TryGetValue(name, out var cached)) return cached;

        var fund = await _db.Funds.FirstOrDefaultAsync(f => f.Name == name && f.PortfolioId == portfolioId && f.OrganizationId == organizationId);
        if (fund == null)
        {
            fund = new Fund { OrganizationId = organizationId, PortfolioId = portfolioId, Name = name, Currency = "USD" };
            _db.Funds.Add(fund);
            await _db.SaveChangesAsync();
        }
        cache[name] = fund;
        return fund;
    }

    private async Task<Strategy> GetOrCreateStrategyAsync(string strategyName, int fundId, int organizationId, Dictionary<string, Strategy> cache)
    {
        var name = strategyName.Trim();
        var key = $"{fundId}:{name}";
        if (cache.TryGetValue(key, out var cached)) return cached;

        var strategy = await _db.Strategies.FirstOrDefaultAsync(s => s.Name == name && s.FundId == fundId && s.OrganizationId == organizationId);
        if (strategy == null)
        {
            strategy = new Strategy { OrganizationId = organizationId, FundId = fundId, Name = name };
            _db.Strategies.Add(strategy);
            await _db.SaveChangesAsync();
        }
        cache[key] = strategy;
        return strategy;
    }
}
