namespace Api.Services.JwtTokenValidation;

using System.Security.Claims;
using Api.Data;
using Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Validates JWTs from Keycloak and ensures a local User record exists for referential integrity.
/// </summary>
public class JwtTokenValidationService(
    ILogger<JwtTokenValidationService> logger,
    ApplicationDbContext db) : IJwtTokenValidationService
{
    private readonly ILogger<JwtTokenValidationService> _logger = logger;
    private readonly ApplicationDbContext _db = db;

    /// <summary>
    /// Handles token validation and ensures a User record exists for cascade deletion support.
    /// </summary>
    public async Task HandleTokenValidationAsync(TokenValidatedContext context)
    {
        try
        {
            // Extract information from Keycloak token
            var keycloakUserIdString = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(keycloakUserIdString))
            {
                _logger.LogError("Token validated but 'sub' claim is missing");
                context.Fail("Invalid token: missing 'sub' claim");
                return;
            }

            // Parse Keycloak user ID as Guid
            if (!Guid.TryParse(keycloakUserIdString, out var userId))
            {
                _logger.LogError("Token validated but 'sub' claim is not a valid Guid: {Sub}", keycloakUserIdString);
                context.Fail("Invalid token: 'sub' claim must be a valid Guid");
                return;
            }

            _logger.LogInformation("Token validated for Keycloak user: {UserId} ({Email})", userId, email);

            // Ensure a local User record exists for referential integrity and cascade deletion
            var userExists = await _db.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                var user = new User { UserId = userId };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Created User record for Keycloak user {UserId}", userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user provisioning");
            context.Fail($"User provisioning error: {ex.Message}");
        }
    }
}
