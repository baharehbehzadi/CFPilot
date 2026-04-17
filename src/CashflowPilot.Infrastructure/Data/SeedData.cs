using CashflowPilot.Domain.Entities;
using CashflowPilot.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        catch
        {
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
            var portfolio = new Portfolio
            {
                OrganizationId = organization.Id,
                Name = "Global Private Equity Fund I",
                Description = "Diversified buyout and growth equity portfolio",
                BaseCurrency = "USD",
                CreatedAt = DateTime.UtcNow
            };
            db.Portfolios.Add(portfolio);
            await db.SaveChangesAsync();

            var funds = new[]
            {
                new Fund { OrganizationId = organization.Id, PortfolioId = portfolio.Id, Name = "Apex Buyout Fund", Vintage = 2019, AssetClass = "Buyout", Currency = "USD" },
                new Fund { OrganizationId = organization.Id, PortfolioId = portfolio.Id, Name = "Nordic Growth Capital", Vintage = 2020, AssetClass = "Growth Equity", Currency = "EUR" },
                new Fund { OrganizationId = organization.Id, PortfolioId = portfolio.Id, Name = "Sunrise Infrastructure", Vintage = 2021, AssetClass = "Infrastructure", Currency = "USD" }
            };
            db.Funds.AddRange(funds);
            await db.SaveChangesAsync();

            db.Strategies.AddRange(
                new Strategy { OrganizationId = organization.Id, FundId = funds[0].Id, Name = "Large-Cap Buyout", Description = "Control buyouts of established businesses" },
                new Strategy { OrganizationId = organization.Id, FundId = funds[1].Id, Name = "Nordic Tech Growth", Description = "Minority growth investments in Nordic technology" },
                new Strategy { OrganizationId = organization.Id, FundId = funds[2].Id, Name = "Renewables", Description = "Solar and wind infrastructure assets" }
            );
            await db.SaveChangesAsync();

            logger.LogInformation("Seed data created for organization {OrgName}", organization.Name);
        }
    }
}
