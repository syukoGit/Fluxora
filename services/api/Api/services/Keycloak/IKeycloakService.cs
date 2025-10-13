namespace Api.Services.Keycloak;

using Api.DTOs.Auth;

/// <summary>
/// Interface for authentication operations with Keycloak.
/// </summary>
public interface IKeycloakService
{
    /// <summary>
    /// Authenticates a user with Keycloak and returns a token.
    /// </summary>
    Task<KeycloakTokenResponseDto> LoginAsync(string username, string password);

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    Task<KeycloakTokenResponseDto> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    Task<bool> LogoutAsync(string refreshToken);

    /// <summary>
    /// Registers a new user in Keycloak.
    /// </summary>
    Task<bool> RegisterAsync(string email, string username, string password, string? firstName, string? lastName);
}
