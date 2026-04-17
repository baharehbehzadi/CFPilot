using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CashflowPilot.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Portfolios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    BaseCurrency = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Portfolios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Portfolios_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UploadedFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    OriginalFileName = table.Column<string>(type: "TEXT", nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UploadedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    FileType = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Funds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    PortfolioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Vintage = table.Column<int>(type: "INTEGER", nullable: true),
                    AssetClass = table.Column<string>(type: "TEXT", nullable: true),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Funds_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ForecastRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    PortfolioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    RunDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    UploadedFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    EntryCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForecastRuns_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForecastRuns_UploadedFiles_UploadedFileId",
                        column: x => x.UploadedFileId,
                        principalTable: "UploadedFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActualRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    PortfolioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ReportingPeriod = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RunDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    UploadedFileId = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    EntryCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActualRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActualRuns_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActualRuns_UploadedFiles_UploadedFileId",
                        column: x => x.UploadedFileId,
                        principalTable: "UploadedFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Strategies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    FundId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strategies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Strategies_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CashflowEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    ForecastRunId = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualRunId = table.Column<int>(type: "INTEGER", nullable: true),
                    PortfolioId = table.Column<int>(type: "INTEGER", nullable: false),
                    FundId = table.Column<int>(type: "INTEGER", nullable: true),
                    StrategyId = table.Column<int>(type: "INTEGER", nullable: true),
                    Period = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EntryType = table.Column<int>(type: "INTEGER", nullable: false),
                    CapitalCalls = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    Distributions = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    FundName = table.Column<string>(type: "TEXT", nullable: true),
                    StrategyName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashflowEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashflowEntries_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashflowEntries_ForecastRuns_ForecastRunId",
                        column: x => x.ForecastRunId,
                        principalTable: "ForecastRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashflowEntries_ActualRuns_ActualRunId",
                        column: x => x.ActualRunId,
                        principalTable: "ActualRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashflowEntries_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CashflowEntries_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VarianceAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    ForecastRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActualRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VarianceAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VarianceAnalyses_ForecastRuns_ForecastRunId",
                        column: x => x.ForecastRunId,
                        principalTable: "ForecastRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VarianceAnalyses_ActualRuns_ActualRunId",
                        column: x => x.ActualRunId,
                        principalTable: "ActualRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Scenarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    ForecastRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CallsAdjustmentPct = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    DistributionsAdjustmentPct = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    TimingShiftMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scenarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scenarios_ForecastRuns_ForecastRunId",
                        column: x => x.ForecastRunId,
                        principalTable: "ForecastRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VarianceEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VarianceAnalysisId = table.Column<int>(type: "INTEGER", nullable: false),
                    PortfolioId = table.Column<int>(type: "INTEGER", nullable: false),
                    FundId = table.Column<int>(type: "INTEGER", nullable: true),
                    FundName = table.Column<string>(type: "TEXT", nullable: true),
                    StrategyId = table.Column<int>(type: "INTEGER", nullable: true),
                    StrategyName = table.Column<string>(type: "TEXT", nullable: true),
                    Period = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ForecastCapitalCalls = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    ActualCapitalCalls = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    ForecastDistributions = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    ActualDistributions = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false),
                    CumulativeVariance = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VarianceEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VarianceEntries_VarianceAnalyses_VarianceAnalysisId",
                        column: x => x.VarianceAnalysisId,
                        principalTable: "VarianceAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommentaryPacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    VarianceAnalysisId = table.Column<int>(type: "INTEGER", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    GeneratedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    ExecutiveSummary = table.Column<string>(type: "TEXT", nullable: false),
                    PositiveVariances = table.Column<string>(type: "TEXT", nullable: false),
                    NegativeVariances = table.Column<string>(type: "TEXT", nullable: false),
                    DelayedDistributions = table.Column<string>(type: "TEXT", nullable: false),
                    HigherThanExpectedCalls = table.Column<string>(type: "TEXT", nullable: false),
                    Watchpoints = table.Column<string>(type: "TEXT", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentaryPacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentaryPacks_VarianceAnalyses_VarianceAnalysisId",
                        column: x => x.VarianceAnalysisId,
                        principalTable: "VarianceAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OrganizationId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", nullable: true),
                    EntityId = table.Column<string>(type: "TEXT", nullable: true),
                    Details = table.Column<string>(type: "TEXT", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IpAddress = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            // Indexes
            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_OrganizationId",
                table: "AspNetUsers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_OrganizationId",
                table: "Portfolios",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_OrganizationId",
                table: "Funds",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_PortfolioId",
                table: "Funds",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_FundId",
                table: "Strategies",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_Strategies_OrganizationId",
                table: "Strategies",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UploadedFiles_OrganizationId",
                table: "UploadedFiles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRuns_OrganizationId",
                table: "ForecastRuns",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRuns_PortfolioId",
                table: "ForecastRuns",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRuns_UploadedFileId",
                table: "ForecastRuns",
                column: "UploadedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_ActualRuns_OrganizationId",
                table: "ActualRuns",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ActualRuns_PortfolioId",
                table: "ActualRuns",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_ActualRuns_UploadedFileId",
                table: "ActualRuns",
                column: "UploadedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_CashflowEntries_OrganizationId",
                table: "CashflowEntries",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_CashflowEntries_ForecastRunId",
                table: "CashflowEntries",
                column: "ForecastRunId");

            migrationBuilder.CreateIndex(
                name: "IX_CashflowEntries_ActualRunId",
                table: "CashflowEntries",
                column: "ActualRunId");

            migrationBuilder.CreateIndex(
                name: "IX_CashflowEntries_PortfolioId",
                table: "CashflowEntries",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_CashflowEntries_FundId",
                table: "CashflowEntries",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_CashflowEntries_StrategyId",
                table: "CashflowEntries",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianceAnalyses_OrganizationId",
                table: "VarianceAnalyses",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianceAnalyses_ForecastRunId",
                table: "VarianceAnalyses",
                column: "ForecastRunId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianceAnalyses_ActualRunId",
                table: "VarianceAnalyses",
                column: "ActualRunId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianceEntries_VarianceAnalysisId",
                table: "VarianceEntries",
                column: "VarianceAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentaryPacks_OrganizationId",
                table: "CommentaryPacks",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentaryPacks_VarianceAnalysisId",
                table: "CommentaryPacks",
                column: "VarianceAnalysisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_OrganizationId",
                table: "Scenarios",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_ForecastRunId",
                table: "Scenarios",
                column: "ForecastRunId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OrganizationId_Timestamp",
                table: "AuditLogs",
                columns: new[] { "OrganizationId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AuditLogs");
            migrationBuilder.DropTable(name: "Scenarios");
            migrationBuilder.DropTable(name: "CommentaryPacks");
            migrationBuilder.DropTable(name: "VarianceEntries");
            migrationBuilder.DropTable(name: "VarianceAnalyses");
            migrationBuilder.DropTable(name: "CashflowEntries");
            migrationBuilder.DropTable(name: "ActualRuns");
            migrationBuilder.DropTable(name: "ForecastRuns");
            migrationBuilder.DropTable(name: "UploadedFiles");
            migrationBuilder.DropTable(name: "Strategies");
            migrationBuilder.DropTable(name: "Funds");
            migrationBuilder.DropTable(name: "Portfolios");
            migrationBuilder.DropTable(name: "AspNetUserTokens");
            migrationBuilder.DropTable(name: "AspNetUserRoles");
            migrationBuilder.DropTable(name: "AspNetUserLogins");
            migrationBuilder.DropTable(name: "AspNetUserClaims");
            migrationBuilder.DropTable(name: "AspNetRoleClaims");
            migrationBuilder.DropTable(name: "AspNetUsers");
            migrationBuilder.DropTable(name: "AspNetRoles");
            migrationBuilder.DropTable(name: "Organizations");
        }
    }
}
