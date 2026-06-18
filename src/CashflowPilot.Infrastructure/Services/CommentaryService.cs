using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Entities;
using CashflowPilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Infrastructure.Services;

public class CommentaryService : ICommentaryService
{
    private readonly ApplicationDbContext _db;
    private readonly ICommentaryGenerator _generator;

    public CommentaryService(ApplicationDbContext db, ICommentaryGenerator generator)
    {
        _db = db;
        _generator = generator;
    }

    public async Task<CommentaryPack> GenerateAsync(int analysisId, string userId, int organizationId)
    {
        var analysis = await _db.VarianceAnalyses
            .Include(v => v.Entries)
            .Include(v => v.ForecastRun)
            .Include(v => v.ActualRun)
            .FirstOrDefaultAsync(v => v.Id == analysisId && v.OrganizationId == organizationId)
            ?? throw new InvalidOperationException($"Variance analysis {analysisId} not found.");

        var settings = await _db.OrganizationSettings.FirstOrDefaultAsync(s => s.OrganizationId == organizationId)
            ?? new OrganizationSettings { OrganizationId = organizationId };

        var content = _generator.Generate(analysis, analysis.Entries.ToList(), settings);

        var pack = new CommentaryPack
        {
            OrganizationId = organizationId,
            VarianceAnalysisId = analysisId,
            GeneratedAt = DateTime.UtcNow,
            GeneratedByUserId = userId,
            ExecutiveSummary = content.ExecutiveSummary,
            PositiveVariances = content.PositiveVariances,
            NegativeVariances = content.NegativeVariances,
            DelayedDistributions = content.DelayedDistributions,
            HigherThanExpectedCalls = content.HigherThanExpectedCalls,
            Watchpoints = content.Watchpoints
        };

        // Remove existing commentary if re-generating
        var existing = await _db.CommentaryPacks.FirstOrDefaultAsync(c => c.VarianceAnalysisId == analysisId);
        if (existing != null) _db.CommentaryPacks.Remove(existing);

        _db.CommentaryPacks.Add(pack);
        await _db.SaveChangesAsync();
        return pack;
    }
}
