namespace Api.Services.JwtTokenValidation;

using System.Security.Claims;
using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Validates JWTs from Keycloak and ensures a local link record exists.
/// </summary>
public class JwtTokenValidationService(
    ILogger<JwtTokenValidationService> logger,
    ApplicationDbContext db) : IJwtTokenValidationService
{
    private readonly ILogger<JwtTokenValidationService> _logger = logger;
    private readonly ApplicationDbContext _db = db;

    /// <summary>
    /// Handles token validation and user provisioning/synchronization
    /// </summary>
    public async Task HandleTokenValidationAsync(TokenValidatedContext context)
    {
        try
        {
            // Extract information from Keycloak token
            var keycloakUserId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;
            var preferredUsername = context.Principal?.FindFirst("preferred_username")?.Value;
            var firstName = context.Principal?.FindFirst(ClaimTypes.GivenName)?.Value;
            var lastName = context.Principal?.FindFirst(ClaimTypes.Surname)?.Value;

            if (string.IsNullOrEmpty(keycloakUserId))
            {
                _logger.LogError("Token validated but 'sub' claim is missing");
                context.Fail("Invalid token: missing 'sub' claim");
                return;
            }

            _logger.LogInformation("Token validated for Keycloak user: {KeycloakUserId} ({Email})", keycloakUserId, email);

            // Ensure a local link exists
            var link = await _db.UserLinks.AsNoTracking().FirstOrDefaultAsync(l => l.KeycloakUserId == keycloakUserId);
            if (link == null)
            {
                link = new UserAccountLink { KeycloakUserId = keycloakUserId };
                _db.UserLinks.Add(link);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Created local link for Keycloak user {KeycloakUserId}", keycloakUserId);
            }

            // No local roles or user profile management; roles come from Keycloak claims
            _logger.LogInformation("Token validated and link ensured for Keycloak user: {KeycloakUserId}", keycloakUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user provisioning from Keycloak");
            context.Fail($"User provisioning error: {ex.Message}");
        }
    }

    // No local user profile or role synchronization in this setup.
}
