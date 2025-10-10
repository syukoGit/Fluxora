namespace Api.Configuration;

/// <summary>
/// Configuration settings for Keycloak integration.
/// </summary>
public class KeycloakSettings
{
    /// <summary>
    /// The Keycloak realm authority URL.
    /// Example: http://localhost:8080/realms/fluxora
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// The realm name in Keycloak.
    /// </summary>
    public string Realm { get; set; } = string.Empty;

    /// <summary>
    /// The audience claim for JWT validation.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// The client ID for user authentication.
    /// </summary>
    public string AuthClientId { get; set; } = string.Empty;

    /// <summary>
    /// The client secret for user authentication (if confidential client).
    /// </summary>
    public string? AuthClientSecret { get; set; }

    /// <summary>
    /// Whether to require HTTPS for metadata retrieval.
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// The OpenID Connect metadata endpoint.
    /// </summary>
    public string MetadataAddress { get; set; } = string.Empty;

    /// <summary>
    /// The client ID for user management operations (service account).
    /// This client should have manage-users and view-users roles.
    /// </summary>
    public string UserManagerClientId { get; set; } = string.Empty;

    /// <summary>
    /// The client secret for the user management service account.
    /// </summary>
    public string UserManagerClientSecret { get; set; } = string.Empty;
}
