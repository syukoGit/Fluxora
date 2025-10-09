namespace Api.Models;

using Microsoft.AspNetCore.Identity;

/// <summary>
/// Rôle de l'application avec des propriétés personnalisées
/// </summary>
public class ApplicationRole : IdentityRole
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
