using CashflowPilot.Application.Interfaces;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/cashflowpilot-.log", rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=cashflowpilot.db"));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Application services
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<ICsvParserService, CsvParserService>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<IVarianceService, VarianceService>();
builder.Services.AddScoped<IScenarioService, ScenarioService>();
builder.Services.AddScoped<ICommentaryService, CommentaryService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IReportExportService, ReportExportService>();

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAnalyst", policy =>
        policy.RequireRole(CashflowPilot.Domain.Enums.Roles.Admin, CashflowPilot.Domain.Enums.Roles.Analyst));
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole(CashflowPilot.Domain.Enums.Roles.Admin));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "login", pattern: "login",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(name: "default", pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// Seed database
await SeedData.InitializeAsync(app.Services);

app.Run();

public partial class Program { }
