using CashflowPilot.Domain.Entities;
using CashflowPilot.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Infrastructure.Data;

public class ApplicationUser : IdentityUser
{
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<Fund> Funds => Set<Fund>();
    public DbSet<Strategy> Strategies => Set<Strategy>();
    public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();
    public DbSet<ForecastRun> ForecastRuns => Set<ForecastRun>();
    public DbSet<ActualRun> ActualRuns => Set<ActualRun>();
    public DbSet<CashflowEntry> CashflowEntries => Set<CashflowEntry>();
    public DbSet<VarianceAnalysis> VarianceAnalyses => Set<VarianceAnalysis>();
    public DbSet<VarianceEntry> VarianceEntries => Set<VarianceEntry>();
    public DbSet<CommentaryPack> CommentaryPacks => Set<CommentaryPack>();
    public DbSet<Scenario> Scenarios => Set<Scenario>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ApplicationUser → Organization
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.Organization)
            .WithMany()
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Organization
        builder.Entity<Organization>()
            .HasIndex(o => o.Name).IsUnique();

        // Portfolio
        builder.Entity<Portfolio>()
            .HasOne(p => p.Organization)
            .WithMany(o => o.Portfolios)
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Portfolio>()
            .HasIndex(p => p.OrganizationId);

        // Fund
        builder.Entity<Fund>()
            .HasOne(f => f.Portfolio)
            .WithMany(p => p.Funds)
            .HasForeignKey(f => f.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Fund>()
            .HasIndex(f => f.OrganizationId);

        // Strategy
        builder.Entity<Strategy>()
            .HasOne(s => s.Fund)
            .WithMany(f => f.Strategies)
            .HasForeignKey(s => s.FundId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Strategy>()
            .HasIndex(s => s.OrganizationId);

        // UploadedFile
        builder.Entity<UploadedFile>()
            .HasIndex(f => f.OrganizationId);

        builder.Entity<UploadedFile>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(f => f.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // ForecastRun
        builder.Entity<ForecastRun>()
            .HasOne(r => r.Portfolio)
            .WithMany(p => p.ForecastRuns)
            .HasForeignKey(r => r.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ForecastRun>()
            .HasOne(r => r.UploadedFile)
            .WithMany()
            .HasForeignKey(r => r.UploadedFileId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.Entity<ForecastRun>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.Entity<ForecastRun>()
            .HasIndex(r => r.OrganizationId);

        // ActualRun
        builder.Entity<ActualRun>()
            .HasOne(r => r.Portfolio)
            .WithMany(p => p.ActualRuns)
            .HasForeignKey(r => r.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ActualRun>()
            .HasOne(r => r.UploadedFile)
            .WithMany()
            .HasForeignKey(r => r.UploadedFileId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.Entity<ActualRun>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.Entity<ActualRun>()
            .HasIndex(r => r.OrganizationId);

        // CashflowEntry - ignore computed property
        builder.Entity<CashflowEntry>()
            .Ignore(e => e.NetCashflow);

        builder.Entity<CashflowEntry>()
            .HasOne(e => e.Portfolio)
            .WithMany()
            .HasForeignKey(e => e.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CashflowEntry>()
            .HasOne(e => e.ForecastRun)
            .WithMany(r => r.CashflowEntries)
            .HasForeignKey(e => e.ForecastRunId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.Entity<CashflowEntry>()
            .HasOne(e => e.ActualRun)
            .WithMany(r => r.CashflowEntries)
            .HasForeignKey(e => e.ActualRunId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.Entity<CashflowEntry>()
            .HasOne(e => e.Fund)
            .WithMany(f => f.CashflowEntries)
            .HasForeignKey(e => e.FundId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.Entity<CashflowEntry>()
            .HasOne(e => e.Strategy)
            .WithMany()
            .HasForeignKey(e => e.StrategyId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.Entity<CashflowEntry>()
            .Property(e => e.CapitalCalls).HasPrecision(18, 6);

        builder.Entity<CashflowEntry>()
            .Property(e => e.Distributions).HasPrecision(18, 6);

        builder.Entity<CashflowEntry>()
            .HasIndex(e => e.OrganizationId);

        // VarianceAnalysis
        builder.Entity<VarianceAnalysis>()
            .HasOne(v => v.ForecastRun)
            .WithMany()
            .HasForeignKey(v => v.ForecastRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<VarianceAnalysis>()
            .HasOne(v => v.ActualRun)
            .WithMany()
            .HasForeignKey(v => v.ActualRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<VarianceAnalysis>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(v => v.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.Entity<VarianceAnalysis>()
            .HasIndex(v => v.OrganizationId);

        // VarianceEntry - ignore all computed properties
        builder.Entity<VarianceEntry>()
            .Ignore(e => e.CapitalCallsVariance)
            .Ignore(e => e.CapitalCallsVariancePct)
            .Ignore(e => e.DistributionsVariance)
            .Ignore(e => e.DistributionsVariancePct)
            .Ignore(e => e.ForecastNetCashflow)
            .Ignore(e => e.ActualNetCashflow)
            .Ignore(e => e.NetCashflowVariance)
            .Ignore(e => e.NetCashflowVariancePct);

        builder.Entity<VarianceEntry>()
            .HasOne(e => e.VarianceAnalysis)
            .WithMany(v => v.Entries)
            .HasForeignKey(e => e.VarianceAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<VarianceEntry>()
            .Property(e => e.ForecastCapitalCalls).HasPrecision(18, 6);

        builder.Entity<VarianceEntry>()
            .Property(e => e.ActualCapitalCalls).HasPrecision(18, 6);

        builder.Entity<VarianceEntry>()
            .Property(e => e.ForecastDistributions).HasPrecision(18, 6);

        builder.Entity<VarianceEntry>()
            .Property(e => e.ActualDistributions).HasPrecision(18, 6);

        builder.Entity<VarianceEntry>()
            .Property(e => e.CumulativeVariance).HasPrecision(18, 6);

        // CommentaryPack - 1:1 with VarianceAnalysis
        builder.Entity<CommentaryPack>()
            .HasOne(c => c.VarianceAnalysis)
            .WithOne(v => v.CommentaryPack)
            .HasForeignKey<CommentaryPack>(c => c.VarianceAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CommentaryPack>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.GeneratedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.Entity<CommentaryPack>()
            .HasIndex(c => c.OrganizationId);

        // Scenario
        builder.Entity<Scenario>()
            .HasOne(s => s.ForecastRun)
            .WithMany()
            .HasForeignKey(s => s.ForecastRunId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Scenario>()
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.Entity<Scenario>()
            .Property(s => s.CallsAdjustmentPct).HasPrecision(10, 4);

        builder.Entity<Scenario>()
            .Property(s => s.DistributionsAdjustmentPct).HasPrecision(10, 4);

        builder.Entity<Scenario>()
            .HasIndex(s => s.OrganizationId);

        // AuditLog — no FK constraints, append-only
        builder.Entity<AuditLog>()
            .HasIndex(a => new { a.OrganizationId, a.Timestamp });
    }
}
