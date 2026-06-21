# CashflowPilot

A production-style private-markets cashflow reporting, variance analysis, and scenario planning web application built with ASP.NET Core 8, Entity Framework Core, and PostgreSQL — sold as a subscription-based SaaS with self-serve signup and Stripe billing.

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
- Sign up self-serve at `/Account/Register` (creates a new organization with a 14-day free trial) and subscribe to a paid plan (Starter / Professional) via Stripe Billing

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (included in Visual Studio 2022 v17.8+)
- [PostgreSQL](https://www.postgresql.org/download/) 14+ running locally or reachable over the network
- A [Stripe](https://dashboard.stripe.com/) account (test mode is fine for development) if you want to exercise the billing flow

## Quick Start

### 1. Clone and open

```bash
git clone <repo-url>
cd CashflowPilot
```

Open `CashflowPilot.sln` in Visual Studio 2022, or use the `dotnet` CLI.

### 2. Create the database

```bash
createdb cashflowpilot_dev
```

### 3. Configure secrets

Copy `appsettings.example.json` to `appsettings.Development.json` in `src/CashflowPilot.Web/` for non-secret defaults, then set the real connection string and Stripe keys with `dotnet user-secrets` (never commit real credentials to an appsettings file):

```bash
cd src/CashflowPilot.Web
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=cashflowpilot_dev;Username=<your-pg-user>;Password=<your-pg-password>"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_..."
dotnet user-secrets set "Stripe:StarterPriceId" "price_..."
dotnet user-secrets set "Stripe:ProfessionalPriceId" "price_..."
```

The Stripe keys are only required to exercise the live checkout/billing-portal/webhook flow; the app runs fine without them, but the Subscribe/Manage Billing actions will fail until they're set. The storage folder (for uploaded CSVs) is created automatically.

### 4. Run

```bash
cd src/CashflowPilot.Web
dotnet run
```

Or press **F5** in Visual Studio.

The app will:
1. Apply all EF Core migrations to the PostgreSQL database automatically
2. Seed demo data: an organization, a multi-strategy portfolio with 5 funds, three user accounts, a pre-imported forecast and actuals run with a variance analysis and generated commentary, a sample scenario, and NAV snapshots for every fund

### 5. Log in

| Email | Password | Role |
|-------|----------|------|
| admin@meridian.example | Admin@123456 | Admin |
| analyst@meridian.example | Analyst@123456 | Analyst |
| viewer@meridian.example | Viewer@123456 | Viewer |

The demo organization is seeded on the Professional plan with an active subscription, so it's never paywalled — use it for demos/sales calls. To try the self-serve signup flow instead, go to `/Account/Register` and create a new company; it starts a 14-day trial automatically.

### 6. Try it out

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
│   │                                    # IVarianceComparisonService, IReportPackService,
│   │                                    # IAuditService, IBillingService
│   ├── CashflowPilot.Infrastructure/   # EF Core (PostgreSQL/Npgsql), services, file storage
│   │   ├── Data/
│   │   │   ├── ApplicationDbContext.cs
│   │   │   ├── SeedData.cs
│   │   │   └── Migrations/
│   │   └── Services/                   # CsvParserService, VarianceService, ScenarioService,
│   │                                    # CommentaryService, RuleBasedCommentaryGenerator,
│   │                                    # NavSnapshotService, VarianceComparisonService,
│   │                                    # ReportPackService, ReportExportService,
│   │                                    # FileStorageService, UploadService, AuditService,
│   │                                    # StripeBillingService
│   └── CashflowPilot.Web/              # ASP.NET Core MVC
│       ├── Controllers/                # Account (login + self-serve registration), Dashboard,
│       │                                # Upload, Analysis, Scenario, Commentary, NavSnapshot,
│       │                                # Admin, Billing, StripeWebhook
│       ├── Filters/                    # SubscriptionGateFilter
│       ├── Models/                     # ViewModels
│       ├── Views/                      # Razor views (Bootstrap 5, Chart.js — vendored locally
│       │                               # under wwwroot/lib, no CDN dependency)
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

## Subscriptions & Billing

Every new organization created via `/Account/Register` starts on a 14-day free trial (`PlanTier.Trial`). Admins can subscribe to **Starter** or **Professional** from the **Billing** page, which redirects to a Stripe Checkout session. A `SubscriptionGateFilter` runs on every request and redirects to the Billing page once a trial expires or a subscription becomes past-due/canceled; only the Account, Billing, and StripeWebhook controllers are exempt so a locked-out org can always get to billing or sign out.

Stripe webhook events are handled at `POST /webhooks/stripe` (signature-verified against `Stripe:WebhookSecret`): `checkout.session.completed`, `customer.subscription.updated`, `customer.subscription.deleted`, and `invoice.payment_failed`. In production this endpoint must be publicly reachable and registered in the Stripe Dashboard (or via the Stripe CLI for local testing: `stripe listen --forward-to localhost:5000/webhooks/stripe`).

## EF Core Migrations

The app automatically applies migrations on startup via `Database.MigrateAsync()`. To add a new migration after schema changes:

```bash
cd src/CashflowPilot.Web
dotnet ef migrations add YourMigrationName --project ../CashflowPilot.Infrastructure --startup-project . --output-dir Data/Migrations
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
- **Web**: ASP.NET Core MVC with Razor views, Bootstrap 5, Chart.js (vendored locally, no CDN dependency), depends on all layers
- **Multi-tenancy**: All data is scoped by `OrganizationId`; users belong to exactly one organization; every service method takes and enforces an `organizationId` parameter
- **Self-serve signup & billing**: `/Account/Register` creates a new `Organization` + admin user in a single transaction and starts a 14-day trial; `Organization` carries `PlanTier`, `SubscriptionStatus`, `TrialEndsAt`, and Stripe customer/subscription IDs. `IBillingService`/`StripeBillingService` create Stripe Checkout and Billing Portal sessions and process webhook events; `SubscriptionGateFilter` enforces access based on subscription status globally
- **Authentication**: ASP.NET Core Identity with cookie auth; roles: Admin, Analyst, Viewer; email addresses are unique platform-wide (`RequireUniqueEmail`)
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
- [ ] Production deployment guide (VPS provisioning, TLS, managed Postgres backups, CI/CD)
- [ ] Per-plan feature/usage limits (e.g. cap funds or users on Starter) enforced server-side, not just shown on the pricing page
- [ ] Automated trial-ending and payment-failed email reminders (currently only an in-app banner)

## Security Notes

- Passwords are hashed by ASP.NET Core Identity (PBKDF2)
- Anti-forgery tokens on all POST forms
- All data queries are scoped to the user's organization
- File uploads are validated and stored with GUID-based names (no original filename used for storage path)
- Input sanitization via model binding and EF Core parameterized queries
