using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Entities;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CashflowPilot.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await db.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "MigrateAsync failed, falling back to EnsureCreatedAsync");
            await db.Database.EnsureCreatedAsync();
        }

        // Seed roles
        foreach (var role in new[] { Roles.Admin, Roles.Analyst, Roles.Viewer })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed organization
        if (!await db.Organizations.AnyAsync())
        {
            var org = new Organization { Name = "Meridian Capital Partners", Description = "Private equity fund manager", CreatedAt = DateTime.UtcNow };
            db.Organizations.Add(org);
            await db.SaveChangesAsync();
        }

        var organization = await db.Organizations.FirstAsync();

        // Seed default organization settings (variance thresholds, watchpoint config)
        if (!await db.OrganizationSettings.AnyAsync(s => s.OrganizationId == organization.Id))
        {
            db.OrganizationSettings.Add(new OrganizationSettings { OrganizationId = organization.Id });
            await db.SaveChangesAsync();
        }

        // Seed users
        async Task EnsureUser(string email, string password, string displayName, string role)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    DisplayName = displayName,
                    OrganizationId = organization.Id,
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, role);
                else
                    logger.LogError("Failed to seed user {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        await EnsureUser("admin@meridian.example", "Admin@123456", "System Administrator", Roles.Admin);
        await EnsureUser("analyst@meridian.example", "Analyst@123456", "Jane Analyst", Roles.Analyst);
        await EnsureUser("viewer@meridian.example", "Viewer@123456", "Bob Viewer", Roles.Viewer);

        // Seed portfolio and funds
        if (!await db.Portfolios.AnyAsync())
        {
            var newPortfolio = new Portfolio
            {
                OrganizationId = organization.Id,
                Name = "Meridian Diversified Alternatives Fund I",
                Description = "Multi-strategy portfolio spanning buyout, growth equity, infrastructure, private credit, and real estate",
                BaseCurrency = "USD",
                CreatedAt = DateTime.UtcNow
            };
            db.Portfolios.Add(newPortfolio);
            await db.SaveChangesAsync();

            var funds = new[]
            {
                new Fund { OrganizationId = organization.Id, PortfolioId = newPortfolio.Id, Name = "Apex Buyout Fund", Vintage = 2019, AssetClass = "Buyout", Currency = "USD" },
                new Fund { OrganizationId = organization.Id, PortfolioId = newPortfolio.Id, Name = "Nordic Growth Capital", Vintage = 2020, AssetClass = "Growth Equity", Currency = "EUR" },
                new Fund { OrganizationId = organization.Id, PortfolioId = newPortfolio.Id, Name = "Sunrise Infrastructure", Vintage = 2021, AssetClass = "Infrastructure", Currency = "USD" },
                new Fund { OrganizationId = organization.Id, PortfolioId = newPortfolio.Id, Name = "Meridian Direct Lending Fund", Vintage = 2022, AssetClass = "Private Credit", Currency = "USD" },
                new Fund { OrganizationId = organization.Id, PortfolioId = newPortfolio.Id, Name = "Beacon Real Estate Partners", Vintage = 2022, AssetClass = "Real Estate", Currency = "GBP" }
            };
            db.Funds.AddRange(funds);
            await db.SaveChangesAsync();

            db.Strategies.AddRange(
                new Strategy { OrganizationId = organization.Id, FundId = funds[0].Id, Name = "Large-Cap Buyout", Description = "Control buyouts of established businesses" },
                new Strategy { OrganizationId = organization.Id, FundId = funds[1].Id, Name = "Nordic Tech Growth", Description = "Minority growth investments in Nordic technology" },
                new Strategy { OrganizationId = organization.Id, FundId = funds[2].Id, Name = "Renewables", Description = "Solar and wind infrastructure assets" },
                new Strategy { OrganizationId = organization.Id, FundId = funds[3].Id, Name = "Direct Lending", Description = "Senior secured loans to middle-market sponsors" },
                new Strategy { OrganizationId = organization.Id, FundId = funds[4].Id, Name = "Value-Add Real Estate", Description = "Acquisition and repositioning of commercial properties" }
            );
            await db.SaveChangesAsync();

            logger.LogInformation("Seed data created for organization {OrgName}", organization.Name);
        }

        var portfolio = await db.Portfolios.FirstAsync(p => p.OrganizationId == organization.Id);

        // Seed demo forecast/actual runs, variance analysis, commentary, and a scenario
        if (!await db.ForecastRuns.AnyAsync(r => r.OrganizationId == organization.Id))
        {
            try
            {
                var analystUser = await userManager.FindByEmailAsync("analyst@meridian.example");
                if (analystUser != null)
                {
                    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
                    var dataDir = Path.Combine(env.ContentRootPath, "..", "..", "data");
                    var forecastCsvPath = Path.Combine(dataDir, "sample_forecast.csv");
                    var actualCsvPath = Path.Combine(dataDir, "sample_actuals.csv");

                    if (File.Exists(forecastCsvPath) && File.Exists(actualCsvPath))
                    {
                        var uploadService = scope.ServiceProvider.GetRequiredService<IUploadService>();
                        var varianceService = scope.ServiceProvider.GetRequiredService<IVarianceService>();
                        var commentaryService = scope.ServiceProvider.GetRequiredService<ICommentaryService>();

                        var forecastFile = ReadAsFormFile(forecastCsvPath, "sample_forecast.csv");
                        var forecastFileId = await uploadService.SaveUploadAsync(forecastFile, FileType.ForecastCsv, portfolio.Id, analystUser.Id, organization.Id);
                        var forecastRunId = await uploadService.ImportForecastAsync(forecastFileId, "FY2024 Budget Forecast", portfolio.Id, analystUser.Id, organization.Id);

                        var actualFile = ReadAsFormFile(actualCsvPath, "sample_actuals.csv");
                        var actualFileId = await uploadService.SaveUploadAsync(actualFile, FileType.ActualCsv, portfolio.Id, analystUser.Id, organization.Id);
                        var actualRunId = await uploadService.ImportActualAsync(actualFileId, "H1 2024 Actuals", new DateTime(2024, 6, 30), portfolio.Id, analystUser.Id, organization.Id);

                        var analysisId = await varianceService.CreateAnalysisAsync(forecastRunId, actualRunId, analystUser.Id, organization.Id, "H1 2024 Variance Review");
                        await commentaryService.GenerateAsync(analysisId, analystUser.Id, organization.Id);

                        db.Scenarios.Add(new Scenario
                        {
                            OrganizationId = organization.Id,
                            ForecastRunId = forecastRunId,
                            Name = "Distribution Slowdown Stress Test",
                            Description = "Models a slower exit environment: distributions delayed and reduced across the portfolio.",
                            CallsAdjustmentPct = 0,
                            DistributionsAdjustmentPct = -20,
                            CallsTimingShiftMonths = 0,
                            DistributionsTimingShiftMonths = 2,
                            Scope = "All",
                            CreatedAt = DateTime.UtcNow,
                            CreatedByUserId = analystUser.Id
                        });
                        await db.SaveChangesAsync();

                        logger.LogInformation("Seeded demo forecast/actual runs, variance analysis, commentary pack, and scenario for organization {OrgName}", organization.Name);
                    }
                    else
                    {
                        logger.LogWarning("Sample CSV files not found at {ForecastPath} / {ActualPath}; skipping demo run seeding.", forecastCsvPath, actualCsvPath);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to seed demo forecast/actual runs, variance analysis, commentary, or scenario.");
            }
        }
    }

    private static IFormFile ReadAsFormFile(string path, string fileName)
    {
        var bytes = File.ReadAllBytes(path);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/csv"
        };
    }
}
