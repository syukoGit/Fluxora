namespace Api.Services.JwtTokenValidation;

using Microsoft.AspNetCore.Authentication.JwtBearer;

/// <summary>
/// Service responsible for JWT token validation and user provisioning from Keycloak
/// </summary>
public interface IJwtTokenValidationService
{
    /// <summary>
    /// Handles token validation and user provisioning/synchronization
    /// </summary>
    /// <param name="context">Token validation context</param>
    /// <returns>Task</returns>
    Task HandleTokenValidationAsync(TokenValidatedContext context);
}
