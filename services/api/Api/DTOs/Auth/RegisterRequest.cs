namespace Api.DTOs.Auth;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// New user registration request.
/// </summary>
public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(25)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must contain at least 8 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [MaxLength(25)]
    public string? FirstName { get; set; }

    [MaxLength(25)]
    public string? LastName { get; set; }
}
