namespace Api.Services.Keycloak;

using System.Text.Json;
using Api.Configuration;
using Api.DTOs.Auth;

/// <summary>
/// Service to manage authentication with Keycloak via HTTP.
/// </summary>
public class KeycloakService(
    HttpClient httpClient,
    KeycloakSettings keycloakSettings,
    ILogger<KeycloakService> logger) : IKeycloakService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly KeycloakSettings _keycloakSettings = keycloakSettings;
    private readonly ILogger<KeycloakService> _logger = logger;

    /// <summary>
    /// Authenticates a user with Keycloak and returns a token.
    /// </summary>
    public async Task<KeycloakTokenResponse> LoginAsync(string username, string password)
    {
        _logger.LogInformation("Attempting login for user: {Username}", username);

        var tokenEndpoint = $"{_keycloakSettings.Authority}/protocol/openid-connect/token";

        var requestData = new Dictionary<string, string>
        {
            ["client_id"] = _keycloakSettings.AuthClientId,
            ["username"] = username,
            ["password"] = password,
            ["grant_type"] = "password"
        };

        if (!string.IsNullOrEmpty(_keycloakSettings.AuthClientSecret))
        {
            requestData["client_secret"] = _keycloakSettings.AuthClientSecret;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(requestData)
        };

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Login failed for user {Username}. Status: {Status}, Error: {Error}",
                    username, response.StatusCode, errorContent);

                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (tokenResponse == null)
            {
                _logger.LogError("Failed to deserialize token response for user: {Username}", username);
                throw new InvalidOperationException("Failed to obtain token from authentication server.");
            }

            _logger.LogInformation("Login successful for user: {Username}", username);
            return tokenResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during login for user: {Username}", username);
            throw new InvalidOperationException("Authentication server is unavailable.", ex);
        }
    }

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    public async Task<KeycloakTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Attempting to refresh token");

        var tokenEndpoint = $"{_keycloakSettings.Authority}/protocol/openid-connect/token";

        var requestData = new Dictionary<string, string>
        {
            ["client_id"] = _keycloakSettings.AuthClientId,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        };

        if (!string.IsNullOrEmpty(_keycloakSettings.AuthClientSecret))
        {
            requestData["client_secret"] = _keycloakSettings.AuthClientSecret;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(requestData)
        };

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Token refresh failed. Status: {Status}, Error: {Error}",
                    response.StatusCode, errorContent);

                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (tokenResponse == null)
            {
                _logger.LogError("Failed to deserialize token refresh response");
                throw new InvalidOperationException("Failed to refresh token.");
            }

            _logger.LogInformation("Token refresh successful");
            return tokenResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during token refresh");
            throw new InvalidOperationException("Authentication server is unavailable.", ex);
        }
    }

    /// <summary>
    /// Revokes a refresh token (logout).
    /// </summary>
    public async Task<bool> LogoutAsync(string refreshToken)
    {
        _logger.LogInformation("Attempting to logout (revoke refresh token)");

        var logoutEndpoint = $"{_keycloakSettings.Authority}/protocol/openid-connect/logout";

        var requestData = new Dictionary<string, string>
        {
            ["client_id"] = _keycloakSettings.AuthClientId,
            ["refresh_token"] = refreshToken
        };

        if (!string.IsNullOrEmpty(_keycloakSettings.AuthClientSecret))
        {
            requestData["client_secret"] = _keycloakSettings.AuthClientSecret;
        }

        var request = new HttpRequestMessage(HttpMethod.Post, logoutEndpoint)
        {
            Content = new FormUrlEncodedContent(requestData)
        };

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Logout failed. Status: {Status}, Error: {Error}",
                    response.StatusCode, errorContent);
                return false;
            }

            _logger.LogInformation("Logout successful");
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during logout");
            return false;
        }
    }

    /// <summary>
    /// Registers a new user in Keycloak.
    /// </summary>
    public async Task<bool> RegisterAsync(string email, string username, string password, string? firstName, string? lastName)
    {
        _logger.LogInformation("Attempting to register user: {Email}", email);

        var realm = string.IsNullOrEmpty(_keycloakSettings.Realm)
            ? _keycloakSettings.Authority.Split("/realms/").Last()
            : _keycloakSettings.Realm;

        // Pour créer un utilisateur, nous devons obtenir un token admin
        var adminToken = await GetAdminTokenAsync();

        if (string.IsNullOrEmpty(adminToken))
        {
            _logger.LogError("Failed to obtain admin token for user registration");
            throw new InvalidOperationException("Unable to register user at this time.");
        }

        // Extraire l'URL de base de Keycloak (sans /realms/xxx)
        var keycloakBaseUrl = _keycloakSettings.Authority.Replace($"/realms/{realm}", "");
        var createUserEndpoint = $"{keycloakBaseUrl}/admin/realms/{realm}/users";

        var userData = new
        {
            username,
            email,
            firstName = firstName ?? string.Empty,
            lastName = lastName ?? string.Empty,
            enabled = true,
            emailVerified = false,
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = password,
                    temporary = false
                }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, createUserEndpoint)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(userData),
                System.Text.Encoding.UTF8,
                "application/json")
        };

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("User registration failed for {Email}. Status: {Status}, Error: {Error}",
                    email, response.StatusCode, errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    throw new InvalidOperationException("A user with this email already exists.");
                }

                throw new InvalidOperationException("Failed to register user.");
            }

            _logger.LogInformation("User registration successful for: {Email}", email);
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during user registration for: {Email}", email);
            throw new InvalidOperationException("Authentication server is unavailable.", ex);
        }
    }

    /// <summary>
    /// Obtains a service account token for performing administrative operations.
    /// Uses the User Management client with client credentials flow.
    /// </summary>
    private async Task<string?> GetAdminTokenAsync()
    {
        if (string.IsNullOrEmpty(_keycloakSettings.UserManagerClientId)
            || string.IsNullOrEmpty(_keycloakSettings.UserManagerClientSecret))
        {
            _logger.LogError("User management client credentials not configured");
            return null;
        }

        var tokenEndpoint = $"{_keycloakSettings.Authority}/protocol/openid-connect/token";

        var requestData = new Dictionary<string, string>
        {
            ["client_id"] = _keycloakSettings.UserManagerClientId,
            ["client_secret"] = _keycloakSettings.UserManagerClientSecret,
            ["grant_type"] = "client_credentials"
        };

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(requestData)
        };

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to obtain service account token. Status: {Status}, Error: {Error}",
                    response.StatusCode, errorContent);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return tokenResponse?.AccessToken;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while obtaining service account token");
            return null;
        }
    }
}
