namespace Api.Models;

using Microsoft.AspNetCore.Identity;

/// <summary>
/// Application role with custom properties
/// </summary>
public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
