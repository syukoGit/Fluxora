namespace Api.Services.Keycloak;

using System.Text.Json;
using Api.DTOs.Auth;

/// <summary>
/// Service to manage authentication with Keycloak via HTTP.
/// </summary>
public class KeycloakService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<KeycloakService> logger) : IKeycloakService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<KeycloakService> _logger = logger;

    /// <summary>
    /// Authenticates a user with Keycloak and returns a token.
    /// </summary>
    public async Task<KeycloakTokenResponse> LoginAsync(string username, string password)
    {
        _logger.LogInformation("Attempting login for user: {Username}", username);

        var authority = _configuration["Keycloak:Authority"];
        var clientId = _configuration["Keycloak:ClientId"] ?? "fluxora-api";
        var clientSecret = _configuration["Keycloak:ClientSecret"];

        var tokenEndpoint = $"{authority}/protocol/openid-connect/token";

        var requestData = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["username"] = username,
            ["password"] = password,
            ["grant_type"] = "password"
        };

        if (!string.IsNullOrEmpty(clientSecret))
        {
            requestData["client_secret"] = clientSecret;
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

        var authority = _configuration["Keycloak:Authority"];
        var clientId = _configuration["Keycloak:ClientId"] ?? "fluxora-api";
        var clientSecret = _configuration["Keycloak:ClientSecret"];

        var tokenEndpoint = $"{authority}/protocol/openid-connect/token";

        var requestData = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        };

        if (!string.IsNullOrEmpty(clientSecret))
        {
            requestData["client_secret"] = clientSecret;
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

        var authority = _configuration["Keycloak:Authority"];
        var clientId = _configuration["Keycloak:ClientId"] ?? "fluxora-api";
        var clientSecret = _configuration["Keycloak:ClientSecret"];

        var logoutEndpoint = $"{authority}/protocol/openid-connect/logout";

        var requestData = new Dictionary<string, string>
        {
            ["client_id"] = clientId,
            ["refresh_token"] = refreshToken
        };

        if (!string.IsNullOrEmpty(clientSecret))
        {
            requestData["client_secret"] = clientSecret;
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
}
