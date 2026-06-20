using CashflowPilot.Application.Interfaces;
using CashflowPilot.Domain.Enums;
using CashflowPilot.Infrastructure.Data;
using CashflowPilot.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CashflowPilot.Web.Controllers;

[Authorize(Policy = "RequireAdmin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuditService _audit;

    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IAuditService audit)
    {
        _db = db;
        _userManager = userManager;
        _audit = audit;
    }

    private async Task<(ApplicationUser user, int orgId)> GetUserAsync()
    {
        var user = await _userManager.GetUserAsync(User) ?? throw new InvalidOperationException("User not found");
        return (user, user.OrganizationId);
    }

    public async Task<IActionResult> Users()
    {
        var (_, orgId) = await GetUserAsync();
        var users = await _db.Users.Where(u => u.OrganizationId == orgId).ToListAsync();
        var vm = new UserListViewModel();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            vm.Users.Add(new UserRowViewModel
            {
                Id = u.Id, Email = u.Email ?? "", DisplayName = u.DisplayName,
                Role = roles.FirstOrDefault() ?? "-", CreatedAt = u.CreatedAt
            });
        }
        return View(vm);
    }

    public IActionResult CreateUser() => View(new CreateUserViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        var (currentUser, orgId) = await GetUserAsync();
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            UserName = model.Email, Email = model.Email, EmailConfirmed = true,
            DisplayName = model.DisplayName, OrganizationId = orgId, CreatedAt = DateTime.UtcNow
        };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors) ModelState.AddModelError(string.Empty, e.Description);
            return View(model);
        }
        await _userManager.AddToRoleAsync(user, model.Role);
        await _audit.LogAsync(orgId, currentUser.Id, currentUser.DisplayName, "CreateUser", "ApplicationUser", user.Id, $"Created user: {model.Email} with role {model.Role}",
            newValue: new { model.Email, model.DisplayName, model.Role });
        TempData["Success"] = $"User {model.Email} created.";
        return RedirectToAction("Users");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var (currentUser, orgId) = await GetUserAsync();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.OrganizationId == orgId);
        if (user != null && user.Id != currentUser.Id)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var oldValue = new { user.Email, user.DisplayName, Role = roles.FirstOrDefault() };
            await _userManager.DeleteAsync(user);
            await _audit.LogAsync(orgId, currentUser.Id, currentUser.DisplayName, "DeleteUser", "ApplicationUser", id, $"Deleted user: {user.Email}", oldValue: oldValue);
        }
        return RedirectToAction("Users");
    }

    public async Task<IActionResult> AuditLog(string? auditAction, string? entityType, string? userId, DateTime? dateFrom, DateTime? dateTo, int page = 1)
    {
        var (_, orgId) = await GetUserAsync();
        int pageSize = 50;
        var query = _db.AuditLogs.Where(a => a.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(auditAction))
            query = query.Where(a => a.Action == auditAction);
        if (!string.IsNullOrWhiteSpace(entityType))
            query = query.Where(a => a.EntityType == entityType);
        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(a => a.UserId == userId);
        if (dateFrom.HasValue)
            query = query.Where(a => a.Timestamp >= dateFrom.Value.Date);
        if (dateTo.HasValue)
            query = query.Where(a => a.Timestamp < dateTo.Value.Date.AddDays(1));

        ViewBag.TotalCount = await query.CountAsync();
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.SelectedAction = auditAction;
        ViewBag.SelectedEntityType = entityType;
        ViewBag.SelectedUserId = userId;
        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");
        ViewBag.Actions = await _db.AuditLogs.Where(a => a.OrganizationId == orgId).Select(a => a.Action).Distinct().OrderBy(a => a).ToListAsync();
        ViewBag.EntityTypes = await _db.AuditLogs.Where(a => a.OrganizationId == orgId && a.EntityType != null).Select(a => a.EntityType).Distinct().OrderBy(a => a).ToListAsync();
        ViewBag.Users = await _db.Users.Where(u => u.OrganizationId == orgId).OrderBy(u => u.DisplayName).Select(u => new { u.Id, u.DisplayName }).ToListAsync();

        var logs = await query.OrderByDescending(a => a.Timestamp).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return View(logs);
    }
}
