using CashflowPilot.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace CashflowPilot.Web.Models;

public class UserListViewModel
{
    public List<UserRowViewModel> Users { get; set; } = new();
}

public class UserRowViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
}

public class CreateUserViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, StringLength(100)] public string DisplayName { get; set; } = string.Empty;
    [Required, MinLength(8)] public string Password { get; set; } = string.Empty;
    [Required] public string Role { get; set; } = CashflowPilot.Domain.Enums.Roles.Analyst;
}
