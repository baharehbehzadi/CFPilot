using System.ComponentModel.DataAnnotations;

namespace CashflowPilot.Web.Models;

public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required, StringLength(200)] public string CompanyName { get; set; } = string.Empty;
    [Required, StringLength(100)] public string DisplayName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
