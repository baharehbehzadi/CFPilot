using CashflowPilot.Application.Interfaces;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Infrastructure.Services;
using CashflowPilot.Web.Filters;
using Microsoft.AspNetCore.HttpOverrides;
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
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.User.RequireUniqueEmail = true;
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
builder.Services.AddScoped<ICommentaryGenerator, RuleBasedCommentaryGenerator>();
builder.Services.AddScoped<ICommentaryService, CommentaryService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IReportExportService, ReportExportService>();
builder.Services.AddScoped<IReportPackService, ReportPackService>();
builder.Services.AddScoped<IVarianceComparisonService, VarianceComparisonService>();
builder.Services.AddScoped<INavSnapshotService, NavSnapshotService>();
builder.Services.AddScoped<IBillingService, StripeBillingService>();
builder.Services.AddScoped<SubscriptionGateFilter>();

// Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAnalyst", policy =>
        policy.RequireRole(CashflowPilot.Domain.Enums.Roles.Admin, CashflowPilot.Domain.Enums.Roles.Analyst));
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole(CashflowPilot.Domain.Enums.Roles.Admin));
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<SubscriptionGateFilter>();
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

app.UseForwardedHeaders();

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
