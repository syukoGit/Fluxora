namespace Api.Models;

/// <summary>
/// Represents a user in the application.
/// This table serves as a reference point for cascade deletion of user data.
/// All user identity and authentication is managed by Keycloak.
/// The UserId is the same as the Keycloak user ID (sub claim).
/// </summary>
public class User
{
    /// <summary>
    /// Keycloak user identifier (sub claim from JWT token as Guid).
    /// This is both the Keycloak ID and the primary key for this table.
    /// </summary>
    public required Guid UserId { get; set; }
}
