using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<string>(type: "text", nullable: true),
                    Details = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    OldValueJson = table.Column<string>(type: "text", nullable: true),
                    NewValueJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    PlanTier = table.Column<int>(type: "integer", nullable: false),
                    SubscriptionStatus = table.Column<int>(type: "integer", nullable: false),
                    TrialEndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StripeCustomerId = table.Column<string>(type: "text", nullable: true),
                    StripeSubscriptionId = table.Column<string>(type: "text", nullable: true),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportingPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PeriodStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PeriodEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Frequency = table.Column<int>(type: "integer", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportingPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
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
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
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
                name: "OrganizationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    MaterialVarianceAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    MaterialVariancePct = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    TopDriverCount = table.Column<int>(type: "integer", nullable: false),
                    WatchpointThresholdPct = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Portfolios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    BaseCurrency = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
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
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
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
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
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
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
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
                name: "UploadedFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    OriginalFileName = table.Column<string>(type: "text", nullable: false),
                    StoredFileName = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UploadedByUserId = table.Column<string>(type: "text", nullable: true),
                    FileType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadedFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UploadedFiles_AspNetUsers_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Funds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    PortfolioId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Vintage = table.Column<int>(type: "integer", nullable: true),
                    AssetClass = table.Column<string>(type: "text", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "ActualRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    PortfolioId = table.Column<int>(type: "integer", nullable: false),
                    ReportingPeriodId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ReportingPeriod = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RunDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UploadedFileId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    EntryCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActualRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActualRuns_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActualRuns_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActualRuns_ReportingPeriods_ReportingPeriodId",
                        column: x => x.ReportingPeriodId,
                        principalTable: "ReportingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ActualRuns_UploadedFiles_UploadedFileId",
                        column: x => x.UploadedFileId,
                        principalTable: "UploadedFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ForecastRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    PortfolioId = table.Column<int>(type: "integer", nullable: false),
                    ReportingPeriodId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    RunDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    UploadedFileId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    EntryCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForecastRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForecastRuns_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForecastRuns_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ForecastRuns_ReportingPeriods_ReportingPeriodId",
                        column: x => x.ReportingPeriodId,
                        principalTable: "ReportingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ForecastRuns_UploadedFiles_UploadedFileId",
                        column: x => x.UploadedFileId,
                        principalTable: "UploadedFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ValidationIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    UploadedFileId = table.Column<int>(type: "integer", nullable: false),
                    Severity = table.Column<int>(type: "integer", nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: true),
                    FieldName = table.Column<string>(type: "text", nullable: true),
                    Message = table.Column<string>(type: "text", nullable: false),
                    SuggestedFix = table.Column<string>(type: "text", nullable: true),
                    IsAccepted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "NavSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    PortfolioId = table.Column<int>(type: "integer", nullable: false),
                    FundId = table.Column<int>(type: "integer", nullable: false),
                    ReportingPeriodId = table.Column<int>(type: "integer", nullable: true),
                    ValuationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NavAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    UnfundedCommitment = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    PaidInCapital = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    TotalDistributions = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    SourceUploadedFileId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NavSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NavSnapshots_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NavSnapshots_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
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
                name: "Strategies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    FundId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                name: "VarianceAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    ForecastRunId = table.Column<int>(type: "integer", nullable: false),
                    ActualRunId = table.Column<int>(type: "integer", nullable: false),
                    ReportingPeriodId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VarianceAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VarianceAnalyses_ActualRuns_ActualRunId",
                        column: x => x.ActualRunId,
                        principalTable: "ActualRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VarianceAnalyses_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VarianceAnalyses_ForecastRuns_ForecastRunId",
                        column: x => x.ForecastRunId,
                        principalTable: "ForecastRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VarianceAnalyses_ReportingPeriods_ReportingPeriodId",
                        column: x => x.ReportingPeriodId,
                        principalTable: "ReportingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CashflowEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    ForecastRunId = table.Column<int>(type: "integer", nullable: true),
                    ActualRunId = table.Column<int>(type: "integer", nullable: true),
                    PortfolioId = table.Column<int>(type: "integer", nullable: false),
                    FundId = table.Column<int>(type: "integer", nullable: true),
                    StrategyId = table.Column<int>(type: "integer", nullable: true),
                    Period = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntryType = table.Column<int>(type: "integer", nullable: false),
                    CapitalCalls = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Distributions = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    FeeAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ExpenseAmount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    FundName = table.Column<string>(type: "text", nullable: true),
                    StrategyName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashflowEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashflowEntries_ActualRuns_ActualRunId",
                        column: x => x.ActualRunId,
                        principalTable: "ActualRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashflowEntries_ForecastRuns_ForecastRunId",
                        column: x => x.ForecastRunId,
                        principalTable: "ForecastRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashflowEntries_Funds_FundId",
                        column: x => x.FundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CashflowEntries_Portfolios_PortfolioId",
                        column: x => x.PortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashflowEntries_Strategies_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Scenarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    ForecastRunId = table.Column<int>(type: "integer", nullable: false),
                    ReportingPeriodId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CallsAdjustmentPct = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    DistributionsAdjustmentPct = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    TimingShiftMonths = table.Column<int>(type: "integer", nullable: false),
                    CallsTimingShiftMonths = table.Column<int>(type: "integer", nullable: false),
                    DistributionsTimingShiftMonths = table.Column<int>(type: "integer", nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: false),
                    ScopePortfolioId = table.Column<int>(type: "integer", nullable: true),
                    ScopeFundId = table.Column<int>(type: "integer", nullable: true),
                    ScopeStrategyId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scenarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scenarios_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Scenarios_ForecastRuns_ForecastRunId",
                        column: x => x.ForecastRunId,
                        principalTable: "ForecastRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Scenarios_Funds_ScopeFundId",
                        column: x => x.ScopeFundId,
                        principalTable: "Funds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Scenarios_Portfolios_ScopePortfolioId",
                        column: x => x.ScopePortfolioId,
                        principalTable: "Portfolios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Scenarios_ReportingPeriods_ReportingPeriodId",
                        column: x => x.ReportingPeriodId,
                        principalTable: "ReportingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Scenarios_Strategies_ScopeStrategyId",
                        column: x => x.ScopeStrategyId,
                        principalTable: "Strategies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CommentaryPacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    VarianceAnalysisId = table.Column<int>(type: "integer", nullable: false),
                    ReportingPeriodId = table.Column<int>(type: "integer", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GeneratedByUserId = table.Column<string>(type: "text", nullable: true),
                    ExecutiveSummary = table.Column<string>(type: "text", nullable: false),
                    PositiveVariances = table.Column<string>(type: "text", nullable: false),
                    NegativeVariances = table.Column<string>(type: "text", nullable: false),
                    DelayedDistributions = table.Column<string>(type: "text", nullable: false),
                    HigherThanExpectedCalls = table.Column<string>(type: "text", nullable: false),
                    Watchpoints = table.Column<string>(type: "text", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentaryPacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentaryPacks_AspNetUsers_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommentaryPacks_ReportingPeriods_ReportingPeriodId",
                        column: x => x.ReportingPeriodId,
                        principalTable: "ReportingPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CommentaryPacks_VarianceAnalyses_VarianceAnalysisId",
                        column: x => x.VarianceAnalysisId,
                        principalTable: "VarianceAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VarianceEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VarianceAnalysisId = table.Column<int>(type: "integer", nullable: false),
                    PortfolioId = table.Column<int>(type: "integer", nullable: false),
                    FundId = table.Column<int>(type: "integer", nullable: true),
                    FundName = table.Column<string>(type: "text", nullable: true),
                    StrategyId = table.Column<int>(type: "integer", nullable: true),
                    StrategyName = table.Column<string>(type: "text", nullable: true),
                    Period = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ForecastCapitalCalls = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ActualCapitalCalls = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ForecastDistributions = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    ActualDistributions = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    CumulativeVariance = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_ActualRuns_CreatedByUserId",
                table: "ActualRuns",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ActualRuns_OrganizationId",
                table: "ActualRuns",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ActualRuns_PortfolioId",
                table: "ActualRuns",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_ActualRuns_ReportingPeriodId",
                table: "ActualRuns",
                column: "ReportingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_ActualRuns_UploadedFileId",
                table: "ActualRuns",
                column: "UploadedFileId");

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
                name: "IX_AspNetUsers_OrganizationId",
                table: "AspNetUsers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OrganizationId_Timestamp",
                table: "AuditLogs",
                columns: new[] { "OrganizationId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_CashflowEntries_ActualRunId",
                table: "CashflowEntries",
                column: "ActualRunId");

            migrationBuilder.CreateIndex(
                name: "IX_CashflowEntries_ForecastRunId",
                table: "CashflowEntries",
                column: "ForecastRunId");

            migrationBuilder.CreateIndex(
                name: "IX_CashflowEntries_FundId",
                table: "CashflowEntries",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_CashflowEntries_OrganizationId",
                table: "CashflowEntries",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_CashflowEntries_PortfolioId",
                table: "CashflowEntries",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_CashflowEntries_StrategyId",
                table: "CashflowEntries",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentaryPacks_GeneratedByUserId",
                table: "CommentaryPacks",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentaryPacks_OrganizationId",
                table: "CommentaryPacks",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentaryPacks_ReportingPeriodId",
                table: "CommentaryPacks",
                column: "ReportingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentaryPacks_VarianceAnalysisId",
                table: "CommentaryPacks",
                column: "VarianceAnalysisId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRuns_CreatedByUserId",
                table: "ForecastRuns",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRuns_OrganizationId",
                table: "ForecastRuns",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRuns_PortfolioId",
                table: "ForecastRuns",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRuns_ReportingPeriodId",
                table: "ForecastRuns",
                column: "ReportingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_ForecastRuns_UploadedFileId",
                table: "ForecastRuns",
                column: "UploadedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_OrganizationId",
                table: "Funds",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Funds_PortfolioId",
                table: "Funds",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_NavSnapshots_FundId",
                table: "NavSnapshots",
                column: "FundId");

            migrationBuilder.CreateIndex(
                name: "IX_NavSnapshots_OrganizationId",
                table: "NavSnapshots",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_NavSnapshots_PortfolioId",
                table: "NavSnapshots",
                column: "PortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_NavSnapshots_ReportingPeriodId",
                table: "NavSnapshots",
                column: "ReportingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_NavSnapshots_SourceUploadedFileId",
                table: "NavSnapshots",
                column: "SourceUploadedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSettings_OrganizationId",
                table: "OrganizationSettings",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_OrganizationId",
                table: "Portfolios",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportingPeriods_OrganizationId",
                table: "ReportingPeriods",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_CreatedByUserId",
                table: "Scenarios",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_ForecastRunId",
                table: "Scenarios",
                column: "ForecastRunId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_OrganizationId",
                table: "Scenarios",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_ReportingPeriodId",
                table: "Scenarios",
                column: "ReportingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_ScopeFundId",
                table: "Scenarios",
                column: "ScopeFundId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_ScopePortfolioId",
                table: "Scenarios",
                column: "ScopePortfolioId");

            migrationBuilder.CreateIndex(
                name: "IX_Scenarios_ScopeStrategyId",
                table: "Scenarios",
                column: "ScopeStrategyId");

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
                name: "IX_UploadedFiles_UploadedByUserId",
                table: "UploadedFiles",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationIssues_OrganizationId",
                table: "ValidationIssues",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationIssues_UploadedFileId",
                table: "ValidationIssues",
                column: "UploadedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianceAnalyses_ActualRunId",
                table: "VarianceAnalyses",
                column: "ActualRunId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianceAnalyses_CreatedByUserId",
                table: "VarianceAnalyses",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianceAnalyses_ForecastRunId",
                table: "VarianceAnalyses",
                column: "ForecastRunId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianceAnalyses_OrganizationId",
                table: "VarianceAnalyses",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianceAnalyses_ReportingPeriodId",
                table: "VarianceAnalyses",
                column: "ReportingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_VarianceEntries_VarianceAnalysisId",
                table: "VarianceEntries",
                column: "VarianceAnalysisId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CashflowEntries");

            migrationBuilder.DropTable(
                name: "CommentaryPacks");

            migrationBuilder.DropTable(
                name: "NavSnapshots");

            migrationBuilder.DropTable(
                name: "OrganizationSettings");

            migrationBuilder.DropTable(
                name: "Scenarios");

            migrationBuilder.DropTable(
                name: "ValidationIssues");

            migrationBuilder.DropTable(
                name: "VarianceEntries");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Strategies");

            migrationBuilder.DropTable(
                name: "VarianceAnalyses");

            migrationBuilder.DropTable(
                name: "Funds");

            migrationBuilder.DropTable(
                name: "ActualRuns");

            migrationBuilder.DropTable(
                name: "ForecastRuns");

            migrationBuilder.DropTable(
                name: "Portfolios");

            migrationBuilder.DropTable(
                name: "ReportingPeriods");

            migrationBuilder.DropTable(
                name: "UploadedFiles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Organizations");
        }
    }
}
