namespace Api.Infrastructure;

using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

/// <summary>
/// Transforme les claims Keycloak pour extraire les rôles du claim realm_access
/// et les ajouter comme claims de rôle standard ASP.NET Core
/// </summary>
public class KeycloakRolesClaimsTransformation(ILogger<KeycloakRolesClaimsTransformation> logger) : IClaimsTransformation
{
    private readonly ILogger<KeycloakRolesClaimsTransformation> _logger = logger;

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Créer une nouvelle identité clonée
        var claimsIdentity = (ClaimsIdentity) principal.Identity!;

        // Vérifier si les rôles ont déjà été transformés
        if (claimsIdentity.HasClaim(c => c.Type == "roles_transformed"))
        {
            return Task.FromResult(principal);
        }

        // Récupérer le claim realm_access
        var realmAccessClaim = claimsIdentity.FindFirst("realm_access");

        if (realmAccessClaim != null)
        {
            try
            {
                // Parser le JSON pour extraire les rôles
                var realmAccess = JsonDocument.Parse(realmAccessClaim.Value);

                if (realmAccess.RootElement.TryGetProperty("roles", out var rolesElement))
                {
                    var roles = rolesElement.EnumerateArray()
                        .Select(r => r.GetString())
                        .Where(r => r != null);

                    foreach (var role in roles)
                    {
                        // Ajouter chaque rôle comme claim standard
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role!));
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse realm_access claim as JSON");
            }
        }

        // Marquer la transformation comme effectuée
        claimsIdentity.AddClaim(new Claim("roles_transformed", "true"));

        return Task.FromResult(principal);
    }
}
