namespace Api.Services.Keycloak;

using System.Text.Json;
using Api.Configuration;
using Api.DTOs.Auth;

/// <inheritdoc />
/// <summary>
/// Service to manage authentication with Keycloak via HTTP.
/// </summary>
public class KeycloakService(HttpClient httpClient, KeycloakSettings keycloakSettings, ILogger<KeycloakService> logger)
    : IKeycloakService
{
    /// <inheritdoc />
    /// <summary>
    /// Authenticates a user with Keycloak and returns a token.
    /// </summary>
    public async Task<KeycloakTokenResponseDto> LoginAsync(string username, string password)
    {
        logger.LogInformation("Attempting login for user: {Username}", username);

        var tokenEndpoint = $"{keycloakSettings.Authority}/protocol/openid-connect/token";

        var requestData = new Dictionary<string, string>
        {
            ["client_id"] = keycloakSettings.AuthClientId,
            ["username"] = username,
            ["password"] = password,
            ["grant_type"] = "password",
        };

        if (!string.IsNullOrEmpty(keycloakSettings.AuthClientSecret))
        {
            requestData["client_secret"] = keycloakSettings.AuthClientSecret;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(requestData),
        };

        try
        {
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();

                logger.LogWarning("Login failed for user {Username}. Status: {Status}, Error: {Error}", username,
                                  response.StatusCode, errorContent);

                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            string content = await response.Content.ReadAsStringAsync();

            var tokenResponse =
                JsonSerializer.Deserialize<KeycloakTokenResponseDto>(
                    content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tokenResponse == null)
            {
                logger.LogError("Failed to deserialize token response for user: {Username}", username);
                throw new InvalidOperationException("Failed to obtain token from authentication server.");
            }

            logger.LogInformation("Login successful for user: {Username}", username);
            return tokenResponse;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error during login for user: {Username}", username);
            throw new InvalidOperationException("Authentication server is unavailable.", ex);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    public async Task<KeycloakTokenResponseDto> RefreshTokenAsync(string refreshToken)
    {
        logger.LogInformation("Attempting to refresh token");

        var tokenEndpoint = $"{keycloakSettings.Authority}/protocol/openid-connect/token";

        var requestData = new Dictionary<string, string>
        {
            ["client_id"] = keycloakSettings.AuthClientId,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token",
        };

        if (!string.IsNullOrEmpty(keycloakSettings.AuthClientSecret))
        {
            requestData["client_secret"] = keycloakSettings.AuthClientSecret;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(requestData),
        };

        try
        {
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();

                logger.LogWarning("Token refresh failed. Status: {Status}, Error: {Error}", response.StatusCode,
                                  errorContent);

                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            string content = await response.Content.ReadAsStringAsync();

            var tokenResponse =
                JsonSerializer.Deserialize<KeycloakTokenResponseDto>(
                    content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tokenResponse == null)
            {
                logger.LogError("Failed to deserialize token refresh response");
                throw new InvalidOperationException("Failed to refresh token.");
            }

            logger.LogInformation("Token refresh successful");
            return tokenResponse;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error during token refresh");
            throw new InvalidOperationException("Authentication server is unavailable.", ex);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Revokes a refresh token (logout).
    /// </summary>
    public async Task<bool> LogoutAsync(string refreshToken)
    {
        logger.LogInformation("Attempting to logout (revoke refresh token)");

        var logoutEndpoint = $"{keycloakSettings.Authority}/protocol/openid-connect/logout";

        var requestData = new Dictionary<string, string>
        {
            ["client_id"] = keycloakSettings.AuthClientId, ["refresh_token"] = refreshToken,
        };

        if (!string.IsNullOrEmpty(keycloakSettings.AuthClientSecret))
        {
            requestData["client_secret"] = keycloakSettings.AuthClientSecret;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, logoutEndpoint)
        {
            Content = new FormUrlEncodedContent(requestData),
        };

        try
        {
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();

                logger.LogWarning("Logout failed. Status: {Status}, Error: {Error}", response.StatusCode, errorContent);

                return false;
            }

            logger.LogInformation("Logout successful");
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error during logout");
            return false;
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Registers a new user in Keycloak.
    /// </summary>
    public async Task<bool> RegisterAsync(string username, string password, string email, string? firstName,
                                          string? lastName)
    {
        logger.LogInformation("Attempting to register user: {UserName}", username);

        string realm = string.IsNullOrEmpty(keycloakSettings.Realm)
                           ? keycloakSettings.Authority.Split("/realms/").Last()
                           : keycloakSettings.Realm;

        // Pour créer un utilisateur, nous devons obtenir un token admin
        string? adminToken = await GetAdminTokenAsync();

        if (string.IsNullOrEmpty(adminToken))
        {
            logger.LogError("Failed to obtain admin token for user registration");
            throw new InvalidOperationException("Unable to register user at this time.");
        }

        // Extraire l'URL de base de Keycloak (sans /realms/xxx)
        string keycloakBaseUrl = keycloakSettings.Authority.Replace($"/realms/{realm}", "");
        var createUserEndpoint = $"{keycloakBaseUrl}/admin/realms/{realm}/users";

        var userData = new
        {
            username,
            email,
            firstName = firstName ?? string.Empty,
            lastName = lastName ?? string.Empty,
            enabled = true,
            emailVerified = false,
            credentials = new[] { new { type = "password", value = password, temporary = false } },
        };

        var request = new HttpRequestMessage(HttpMethod.Post, createUserEndpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(userData), System.Text.Encoding.UTF8,
                                        "application/json"),
        };

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        try
        {
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();

                logger.LogWarning("User registration failed for {UserName}. Status: {Status}, Error: {Error}", username,
                                  response.StatusCode, errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    throw new InvalidOperationException("A user with this email already exists.");
                }

                throw new InvalidOperationException("Failed to register user.");
            }

            logger.LogInformation("User registration successful for: {UserName}", username);
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error during user registration for: {UserName}", username);
            throw new HttpRequestException("Authentication server is unavailable.", ex);
        }
    }

    /// <summary>
    /// Obtains a service account token for performing administrative operations.
    /// Uses the User Management client with client credentials flow.
    /// </summary>
    private async Task<string?> GetAdminTokenAsync()
    {
        if (string.IsNullOrEmpty(keycloakSettings.UserManagerClientId)
         || string.IsNullOrEmpty(keycloakSettings.UserManagerClientSecret))
        {
            logger.LogError("User management client credentials not configured");
            return null;
        }

        var tokenEndpoint = $"{keycloakSettings.Authority}/protocol/openid-connect/token";

        var requestData = new Dictionary<string, string>
        {
            ["client_id"] = keycloakSettings.UserManagerClientId,
            ["client_secret"] = keycloakSettings.UserManagerClientSecret,
            ["grant_type"] = "client_credentials",
        };

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(requestData),
        };

        try
        {
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();

                logger.LogError("Failed to obtain service account token. Status: {Status}, Error: {Error}",
                                response.StatusCode, errorContent);

                return null;
            }

            string content = await response.Content.ReadAsStringAsync();

            var tokenResponse =
                JsonSerializer.Deserialize<KeycloakTokenResponseDto>(
                    content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return tokenResponse?.AccessToken;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error while obtaining service account token");
            return null;
        }
    }
}
