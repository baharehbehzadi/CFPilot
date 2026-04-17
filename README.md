# CashflowPilot

A production-style private-markets cashflow reporting, variance analysis, and scenario planning web application built with ASP.NET Core 8, Entity Framework Core, and SQLite.

## Overview

CashflowPilot helps private equity and private credit fund managers:
- Upload forecast and actual cashflow CSVs
- Compare actuals vs forecasts with variance analysis
- Generate deterministic written commentary
- Run scenario analysis (adjust calls, distributions, and timing)
- Export results to CSV and Excel
- Maintain a full audit trail

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
3. Seed demo data (organization, portfolios, funds, and three user accounts)
4. Start on `https://localhost:5001`

### 4. Log in

| Email | Password | Role |
|-------|----------|------|
| admin@meridian.example | Admin@123456 | Admin |
| analyst@meridian.example | Analyst@123456 | Analyst |
| viewer@meridian.example | Viewer@123456 | Viewer |

### 5. Try it out

1. Go to **Upload Data → Upload Forecast**, upload `data/sample_forecast.csv`
2. Go to **Upload Data → Upload Actuals**, upload `data/sample_actuals.csv`
3. Go to **Variance Analysis → New Analysis**, select the two runs
4. Click **Generate Commentary** on the analysis detail page
5. Export to CSV or Excel

## Project Structure

```
CashflowPilot/
├── CashflowPilot.sln
├── data/
│   ├── sample_forecast.csv     # Sample forecast data (3 funds, 12 months)
│   └── sample_actuals.csv      # Sample actuals (3 funds, 6 months with variances)
├── src/
│   ├── CashflowPilot.Domain/           # Entities, enums, no dependencies
│   │   ├── Entities/                   # Organization, Portfolio, Fund, ...
│   │   └── Enums/                      # RunStatus, EntryType, Roles, ...
│   ├── CashflowPilot.Application/      # Service interfaces, DTOs
│   │   ├── DTOs/                       # ParseResultDto, VarianceSummaryDto, ...
│   │   └── Interfaces/                 # IVarianceService, IScenarioService, ...
│   ├── CashflowPilot.Infrastructure/   # EF Core, services, file storage
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── SeedData.cs
│   │   │   └── Migrations/
│   │   └── Services/                   # CsvParserService, VarianceService, ...
│   └── CashflowPilot.Web/              # ASP.NET Core MVC
│       ├── Controllers/                # Account, Dashboard, Upload, Analysis, ...
│       ├── Models/                     # ViewModels
│       ├── Views/                      # Razor views (Bootstrap 5)
│       ├── Program.cs
│       └── appsettings.json
└── tests/
    └── CashflowPilot.Tests/
        ├── Unit/                       # Parser, variance, scenario tests
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

## Architecture Notes

- **Domain**: Pure C# entities with no framework dependencies
- **Application**: Service interfaces and DTOs, depends only on Domain
- **Infrastructure**: EF Core implementation, CSV parsing (CsvHelper), Excel export (ClosedXML), depends on Application + Domain
- **Web**: ASP.NET Core MVC with Razor views, Bootstrap 5, Chart.js, depends on all layers
- **Multi-tenancy**: All data is scoped by `OrganizationId`; users belong to exactly one organization
- **Authentication**: ASP.NET Core Identity with cookie auth; roles: Admin, Analyst, Viewer
- **Audit logging**: All significant actions are logged to the `AuditLogs` table

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

## Roadmap (Future Enhancements)

- [ ] PDF export via headless browser or reporting library
- [ ] LLM-assisted commentary generation (optional, pluggable)
- [ ] Multi-currency conversion with FX rates
- [ ] Portfolio-level benchmark comparisons
- [ ] Email digest of variance reports
- [ ] Role-based data access (users see only their assigned portfolios)
- [ ] Time-series charts on the dashboard
- [ ] Bulk upload of multiple CSVs
- [ ] API endpoints for programmatic access
- [ ] Azure/AWS deployment guide

## Security Notes

- Passwords are hashed by ASP.NET Core Identity (PBKDF2)
- Anti-forgery tokens on all POST forms
- All data queries are scoped to the user's organization
- File uploads are validated and stored with GUID-based names (no original filename used for storage path)
- Input sanitization via model binding and EF Core parameterized queries
