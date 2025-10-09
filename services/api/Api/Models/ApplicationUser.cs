namespace Api.Models;

using Microsoft.AspNetCore.Identity;

/// <summary>
/// Application user with custom properties
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Link with Keycloak
    public string? KeycloakUserId { get; set; }
}
