# CashflowPilot

A production-style private-markets cashflow reporting, variance analysis, and scenario planning web application built with ASP.NET Core 8, Entity Framework Core, and SQLite.

## Overview

CashflowPilot helps private equity and private credit fund managers:
- Upload forecast and actual cashflow CSVs, with auto column-detection and severity-tiered validation (errors block a row from importing; warnings/info are flagged for review and can be explicitly accepted)
- Track NAV, paid-in capital, distributions, and unfunded commitment per fund over time
- Compare actuals vs. forecasts with variance analysis by period and by fund, including cumulative variance
- Generate deterministic, rule-based written commentary (pluggable via `ICommentaryGenerator`)
- Run scenario analysis: independent percentage adjustments and timing shifts for capital calls vs. distributions, scoped to the whole portfolio, a single fund, or a strategy
- Produce a full reporting pack (cover page, KPIs, top drivers, commentary, scenario summary, data quality, audit trail) for printing or saving as PDF from the browser
- Export variance results to CSV and Excel
- Maintain a full, filterable audit trail
- Enforce role-based access (Admin / Analyst / Viewer), with all data scoped per organization

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (included in Visual Studio 2022 v17.8+)
- No other dependencies — uses SQLite (file-based, no server required)

## Quick Start

### 1. Clone and open

```bash
git clone <repo-url>
cd CashflowPilot
```

Open `CashflowPilot.sln` in Visual Studio 2022, or use the `dotnet` CLI.

### 2. Configure (optional)

Copy `appsettings.example.json` to `appsettings.Development.json` in `src/CashflowPilot.Web/` and adjust:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=cashflowpilot-dev.db"
  },
  "Storage": {
    "Root": "storage-dev"
  }
}
```

The SQLite database file is created automatically. The storage folder is created automatically.

### 3. Run

```bash
cd src/CashflowPilot.Web
dotnet run
```

Or press **F5** in Visual Studio.

The app will:
1. Create the SQLite database (`cashflowpilot-dev.db` by default)
2. Apply all migrations automatically
3. Seed demo data: an organization, a multi-strategy portfolio with 5 funds, three user accounts, a pre-imported forecast and actuals run with a variance analysis and generated commentary, a sample scenario, and NAV snapshots for every fund

### 4. Log in

| Email | Password | Role |
|-------|----------|------|
| admin@meridian.example | Admin@123456 | Admin |
| analyst@meridian.example | Analyst@123456 | Analyst |
| viewer@meridian.example | Viewer@123456 | Viewer |

### 5. Try it out

The demo organization ("Meridian Capital Partners") is pre-seeded with a forecast run, an actuals run, a variance analysis with generated commentary, a sample scenario, and NAV snapshots for all 5 funds — so the dashboard, variance analysis, and reporting pack are populated immediately after logging in. To walk through the flow from scratch:

1. Go to **Upload Data → Upload Forecast**, upload `data/sample_forecast.csv`
2. Go to **Upload Data → Upload Actuals**, upload `data/sample_actuals.csv`
3. Review any flagged rows on the **Validation Issues** screen (errors vs. warnings) and accept or dismiss them as needed
4. Go to **Variance Analysis → New Analysis**, select the two runs
5. Click **Generate Commentary** on the analysis detail page
6. Go to **Scenarios → New Scenario** to model an adjustment (e.g. distributions delayed 2 months and reduced 20%, scoped to one fund) and view the baseline-vs-scenario chart
7. Go to **NAV Snapshots** to record or review a fund's NAV, paid-in capital, distributions, and unfunded commitment
8. Open **Print Report** on the analysis for the full reporting pack, or export the analysis to CSV/Excel
9. As Admin, review **Audit Log**, filterable by action, entity type, user, and date range

## Project Structure

```
CashflowPilot/
├── CashflowPilot.sln
├── data/
│   ├── sample_forecast.csv     # Sample forecast data (3 funds, 12 months)
│   └── sample_actuals.csv      # Sample actuals (3 funds, 6 months with variances)
├── src/
│   ├── CashflowPilot.Domain/           # Entities, enums, no dependencies
│   │   ├── Entities/                   # Organization, Portfolio, Fund, Strategy, CashflowEntry,
│   │   │                               # ForecastRun, ActualRun, VarianceAnalysis, Scenario,
│   │   │                               # CommentaryPack, NavSnapshot, ReportingPeriod,
│   │   │                               # ValidationIssue, OrganizationSettings, AuditLog, ...
│   │   └── Enums/                      # RunStatus, EntryType, FileType, ValidationSeverity, Roles, ...
│   ├── CashflowPilot.Application/      # Service interfaces, DTOs
│   │   ├── DTOs/                       # ParseResultDto, VarianceSummaryDto, ScenarioResultDto,
│   │   │                               # NavSnapshotDto, VarianceComparisonDto, ReportPackDto, ...
│   │   └── Interfaces/                 # IVarianceService, IScenarioService, ICommentaryService,
│   │                                    # ICommentaryGenerator, INavSnapshotService,
│   │                                    # IVarianceComparisonService, IReportPackService, IAuditService
│   ├── CashflowPilot.Infrastructure/   # EF Core, services, file storage
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── SeedData.cs
│   │   │   └── Migrations/
│   │   └── Services/                   # CsvParserService, VarianceService, ScenarioService,
│   │                                    # CommentaryService, RuleBasedCommentaryGenerator,
│   │                                    # NavSnapshotService, VarianceComparisonService,
│   │                                    # ReportPackService, ReportExportService,
│   │                                    # FileStorageService, UploadService, AuditService
│   └── CashflowPilot.Web/              # ASP.NET Core MVC
│       ├── Controllers/                # Account, Dashboard, Upload, Analysis, Scenario,
│       │                                # Commentary, NavSnapshot, Admin
│       ├── Models/                     # ViewModels
│       ├── Views/                      # Razor views (Bootstrap 5, Chart.js)
│       ├── Program.cs
│       └── appsettings.json
└── tests/
    └── CashflowPilot.Tests/
        ├── Unit/                       # Parser, variance, scenario, NAV, commentary,
        │                               # report export/pack, audit service tests
        └── Integration/                # Upload/import flow tests
```

## Storage

Uploaded CSV files are stored on the local filesystem under the path configured in `Storage:Root` (default: `storage/` relative to the web project). Sub-folders are organized as `uploads/{organizationId}/`. No cloud storage is required.

## EF Core Migrations

The app automatically applies migrations on startup via `Database.MigrateAsync()`. To add a new migration after schema changes:

```bash
cd src/CashflowPilot.Web
dotnet ef migrations add YourMigrationName --project ../CashflowPilot.Infrastructure --startup-project .
```

## Running Tests

```bash
dotnet test tests/CashflowPilot.Tests/CashflowPilot.Tests.csproj
```

Tests use xUnit, FluentAssertions, and the EF Core in-memory provider (no mocking framework). Coverage spans CSV parsing, variance and scenario calculations, NAV snapshots, commentary generation, report export/pack assembly, audit logging, and multi-tenant organization isolation, plus end-to-end upload→import integration flows.

## Architecture Notes

- **Domain**: Pure C# entities with no framework dependencies
- **Application**: Service interfaces and DTOs, depends only on Domain
- **Infrastructure**: EF Core implementation, CSV parsing (CsvHelper), Excel export (ClosedXML), depends on Application + Domain
- **Web**: ASP.NET Core MVC with Razor views, Bootstrap 5, Chart.js, depends on all layers
- **Multi-tenancy**: All data is scoped by `OrganizationId`; users belong to exactly one organization; every service method takes and enforces an `organizationId` parameter
- **Authentication**: ASP.NET Core Identity with cookie auth; roles: Admin, Analyst, Viewer
- **Audit logging**: Significant actions (uploads, imports, parsing, validation, scenario creation, commentary generation, role changes) are logged to the `AuditLogs` table, with before/after values where applicable; viewable and filterable on the Admin Audit Log page
- **Commentary generation**: Rule-based and deterministic (no LLM/AI), implemented behind `ICommentaryGenerator` so the rule engine can be swapped or extended without touching `CommentaryService`
- **Variance comparison**: `IVarianceComparisonService` / `VarianceComparisonService` provide forecast-vs-actual, forecast-vs-forecast, and scenario-vs-baseline comparisons; currently exercised by tests and registered for DI, but not yet wired into a dedicated UI page (see Roadmap)

## CSV Format

CashflowPilot auto-detects column headers (case-insensitive). Supported column names:

| Field | Accepted Header Names |
|-------|----------------------|
| Fund Name | Fund, Fund Name, FundName, fund_name |
| Strategy | Strategy, StrategyName, strategy_name |
| Period | Period, Date, Month, reporting_period |
| Capital Calls | Capital Calls, CapitalCalls, capital_calls, Calls, Contributions |
| Distributions | Distributions, Distribution, Dist |
| Currency | Currency, CCY |
| Notes | Notes, Comments, Comment |

If auto-detection fails, you'll be shown a manual column-mapping step.

**Supported period formats**: `yyyy-MM`, `yyyy-MM-dd`, `MM/yyyy`, `MMM-yyyy`, `MMM yyyy`, `Q1 2024`, `Q2-2023`

**Supported amount formats**: plain numbers, comma-separated (`1,500,000`), with currency symbols (`$1,500,000`), accounting notation `(1,500,000)` for negatives.

## Validation Severity

Each parsed row is classified with a severity:

- **Error** — the row cannot be imported (e.g. an unparseable period or amount). Rows with errors are excluded from the run.
- **Warning** — the row imports, but is flagged for review (e.g. a blank fund name, a negative capital call).
- **Info** — informational notices that don't affect import.

Validation issues are persisted per upload and reviewable on the **Validation Issues** screen, where warnings and info items can be explicitly accepted (acknowledged) without blocking the import.

## Roadmap (Future Enhancements)

- [ ] Surface `IVarianceComparisonService` (forecast-vs-forecast, scenario-vs-baseline comparisons) in a dedicated UI page
- [ ] PDF export via headless browser or reporting library (currently print-to-PDF from the browser)
- [ ] LLM-assisted commentary generation (optional, pluggable alongside the existing rule-based generator)
- [ ] Multi-currency conversion with FX rates (funds currently track NAV/cashflows in their native currency without consolidation)
- [ ] Portfolio-level benchmark comparisons
- [ ] Email digest of variance reports
- [ ] Role-based data access at the portfolio level (users currently see all portfolios within their organization)
- [ ] Time-series trend charts on the dashboard (variance and scenario charts already exist on their respective detail pages)
- [ ] Bulk upload of multiple CSVs in one batch
- [ ] API endpoints for programmatic access
- [ ] Azure/AWS deployment guide

## Security Notes

- Passwords are hashed by ASP.NET Core Identity (PBKDF2)
- Anti-forgery tokens on all POST forms
- All data queries are scoped to the user's organization
- File uploads are validated and stored with GUID-based names (no original filename used for storage path)
- Input sanitization via model binding and EF Core parameterized queries
