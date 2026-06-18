using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashflowPilot.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PlatformUpgradeV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportingPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Frequency = table.Column<int>(type: "INTEGER", nullable: false),
                    IsClosed = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaterialVarianceAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    MaterialVariancePct = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    TopDriverCount = table.Column<int>(type: "INTEGER", nullable: false),
                    WatchpointThresholdPct = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationSettings_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddColumn<int>(
                name: "ReportingPeriodId",
                table: "ForecastRuns",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportingPeriodId",
                table: "ActualRuns",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportingPeriodId",
                table: "VarianceAnalyses",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportingPeriodId",
                table: "CommentaryPacks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportingPeriodId",
                table: "Scenarios",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CallsTimingShiftMonths",
                table: "Scenarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DistributionsTimingShiftMonths",
                table: "Scenarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Scope",
                table: "Scenarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "All");

            migrationBuilder.AddColumn<int>(
                name: "ScopePortfolioId",
                table: "Scenarios",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScopeFundId",
                table: "Scenarios",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScopeStrategyId",
                table: "Scenarios",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeAmount",
                table: "CashflowEntries",
                type: "TEXT",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExpenseAmount",
                table: "CashflowEntries",
                type: "TEXT",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "OldValueJson",
                table: "AuditLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewValueJson",
                table: "AuditLogs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NavSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    PortfolioId = table.Column<int>(type: "INTEGER", nullable: false),
                    FundId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReportingPeriodId = table.Column<int>(type: "INTEGER", nullable: true),
                    ValuationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NavAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    UnfundedCommitment = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    PaidInCapital = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    TotalDistributions = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    SourceUploadedFileId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NavSnapshots_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NavSnapshots_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NavSnapshots_ReportingPeriods_ReportingPeriodId",
                        column: x => x.ReportingPeriodId,
                        principalTable: "ReportingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_NavSnapshots_UploadedFiles_SourceUploadedFileId",
                        column: x => x.SourceUploadedFileId,
                        principalTable: "UploadedFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ValidationIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    UploadedFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false),
                    RowNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    FieldName = table.Column<string>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    SuggestedFix = table.Column<string>(type: "TEXT", nullable: true),
                    IsAccepted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidationIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValidationIssues_UploadedFiles_UploadedFileId",
                        column: x => x.UploadedFileId,
                        principalTable: "UploadedFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Indexes
            migrationBuilder.CreateIndex(
                name: "IX_ReportingPeriods_OrganizationId",
                table: "ReportingPeriods",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSettings_OrganizationId",
                table: "OrganizationSettings",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRuns_ReportingPeriodId",
                table: "ForecastRuns",
                column: "ReportingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_ActualRuns_ReportingPeriodId",
                table: "ActualRuns",
                column: "ReportingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianceAnalyses_ReportingPeriodId",
                table: "VarianceAnalyses",
                column: "ReportingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentaryPacks_ReportingPeriodId",
                table: "CommentaryPacks",
                column: "ReportingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_ReportingPeriodId",
                table: "Scenarios",
                column: "ReportingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_ScopePortfolioId",
                table: "Scenarios",
                column: "ScopePortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_ScopeFundId",
                table: "Scenarios",
                column: "ScopeFundId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_ScopeStrategyId",
                table: "Scenarios",
                column: "ScopeStrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_NavSnapshots_OrganizationId",
                table: "NavSnapshots",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_NavSnapshots_PortfolioId",
                table: "NavSnapshots",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_NavSnapshots_FundId",
                table: "NavSnapshots",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_NavSnapshots_ReportingPeriodId",
                table: "NavSnapshots",
                column: "ReportingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_NavSnapshots_SourceUploadedFileId",
                table: "NavSnapshots",
                column: "SourceUploadedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationIssues_OrganizationId",
                table: "ValidationIssues",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationIssues_UploadedFileId",
                table: "ValidationIssues",
                column: "UploadedFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ValidationIssues");
            migrationBuilder.DropTable(name: "NavSnapshots");

            migrationBuilder.DropIndex(name: "IX_Scenarios_ScopeStrategyId", table: "Scenarios");
            migrationBuilder.DropIndex(name: "IX_Scenarios_ScopeFundId", table: "Scenarios");
            migrationBuilder.DropIndex(name: "IX_Scenarios_ScopePortfolioId", table: "Scenarios");
            migrationBuilder.DropIndex(name: "IX_Scenarios_ReportingPeriodId", table: "Scenarios");
            migrationBuilder.DropIndex(name: "IX_CommentaryPacks_ReportingPeriodId", table: "CommentaryPacks");
            migrationBuilder.DropIndex(name: "IX_VarianceAnalyses_ReportingPeriodId", table: "VarianceAnalyses");
            migrationBuilder.DropIndex(name: "IX_ActualRuns_ReportingPeriodId", table: "ActualRuns");
            migrationBuilder.DropIndex(name: "IX_ForecastRuns_ReportingPeriodId", table: "ForecastRuns");

            migrationBuilder.DropColumn(name: "NewValueJson", table: "AuditLogs");
            migrationBuilder.DropColumn(name: "OldValueJson", table: "AuditLogs");
            migrationBuilder.DropColumn(name: "ExpenseAmount", table: "CashflowEntries");
            migrationBuilder.DropColumn(name: "FeeAmount", table: "CashflowEntries");
            migrationBuilder.DropColumn(name: "ScopeStrategyId", table: "Scenarios");
            migrationBuilder.DropColumn(name: "ScopeFundId", table: "Scenarios");
            migrationBuilder.DropColumn(name: "ScopePortfolioId", table: "Scenarios");
            migrationBuilder.DropColumn(name: "Scope", table: "Scenarios");
            migrationBuilder.DropColumn(name: "DistributionsTimingShiftMonths", table: "Scenarios");
            migrationBuilder.DropColumn(name: "CallsTimingShiftMonths", table: "Scenarios");
            migrationBuilder.DropColumn(name: "ReportingPeriodId", table: "Scenarios");
            migrationBuilder.DropColumn(name: "ReportingPeriodId", table: "CommentaryPacks");
            migrationBuilder.DropColumn(name: "ReportingPeriodId", table: "VarianceAnalyses");
            migrationBuilder.DropColumn(name: "ReportingPeriodId", table: "ActualRuns");
            migrationBuilder.DropColumn(name: "ReportingPeriodId", table: "ForecastRuns");

            migrationBuilder.DropTable(name: "OrganizationSettings");
            migrationBuilder.DropTable(name: "ReportingPeriods");
        }
    }
}
