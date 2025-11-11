namespace Api.Infrastructure;

using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

/// <inheritdoc />
/// <summary>
/// Transforms Keycloak claims to extract roles from the realm_access claim
/// and add them as standard ASP.NET Core role claims
/// </summary>
public class KeycloakRolesClaimsTransformation(ILogger<KeycloakRolesClaimsTransformation> logger)
    : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Create a new cloned identity
        if (principal.Identity is not ClaimsIdentity claimsIdentity)
        {
            return Task.FromResult(principal);
        }

        // Check if roles have already been transformed
        if (claimsIdentity.HasClaim(c => c.Type == "roles_transformed"))
        {
            return Task.FromResult(principal);
        }

        // Get the realm_access claim
        var realmAccessClaim = claimsIdentity.FindFirst("realm_access");

        if (realmAccessClaim != null)
        {
            try
            {
                // Parse the JSON to extract roles
                var realmAccess = JsonDocument.Parse(realmAccessClaim.Value);

                if (realmAccess.RootElement.TryGetProperty("roles", out var rolesElement))
                {
                    var roles = rolesElement.EnumerateArray().Select(r => r.GetString()).Where(r => r != null);

                    foreach (string role in roles.OfType<string>())
                    {
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to parse realm_access claim as JSON");
            }
        }

        // Mark the transformation as completed
        claimsIdentity.AddClaim(new Claim("roles_transformed", "true"));

        return Task.FromResult(principal);
    }
}
