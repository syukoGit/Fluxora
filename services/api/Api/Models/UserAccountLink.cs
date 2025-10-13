namespace Api.Models;

/// <summary>
/// Minimal link between a local user identifier and the Keycloak user ID (sub).
/// </summary>
public class UserAccountLink
{
    // Local application user ID (internal reference used by other domain entities)
    public Guid Id { get; set; } = Guid.NewGuid();

    // Keycloak subject identifier (user ID in Keycloak)
    public string KeycloakUserId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
