namespace Api.Controllers;

using System.Security.Claims;
using Api.DTOs.Auth;
using Api.Services.Keycloak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Contrôleur d'authentification.
/// Proxifie les appels à Keycloak pour masquer l'infrastructure sous-jacente.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IKeycloakService keycloakService,
    ILogger<AuthController> logger) : ControllerBase
{
    private readonly IKeycloakService _keycloakService = keycloakService;
    private readonly ILogger<AuthController> _logger = logger;

    /// <summary>
    /// Authentifie un utilisateur et retourne un token JWT.
    /// </summary>
    /// <param name="request">Les identifiants de l'utilisateur.</param>
    /// <response code="200">Authentification réussie, token retourné.</response>
    /// <response code="400">Requête invalide.</response>
    /// <response code="401">Identifiants incorrects.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(KeycloakTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var tokenResponse = await _keycloakService.LoginAsync(request.Username, request.Password);
            return Ok(tokenResponse);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Login failed for user: {Username}", request.Username);
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Authentication service error for user: {Username}", request.Username);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Rafraîchit un access token expiré en utilisant un refresh token.
    /// </summary>
    /// <param name="request">Le refresh token.</param>
    /// <response code="200">Token rafraîchi avec succès.</response>
    /// <response code="400">Requête invalide.</response>
    /// <response code="401">Refresh token invalide ou expiré.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(KeycloakTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var tokenResponse = await _keycloakService.RefreshTokenAsync(request.RefreshToken);
            return Ok(tokenResponse);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Token refresh failed");
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Authentication service error during token refresh");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Déconnecte l'utilisateur en révoquant son refresh token.
    /// </summary>
    /// <param name="request">Le refresh token à révoquer.</param>
    /// <response code="200">Déconnexion réussie.</response>
    /// <response code="400">Requête invalide.</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _keycloakService.LogoutAsync(request.RefreshToken);

        if (success)
        {
            return Ok(new { message = "Logout successful" });
        }

        return Ok(new { message = "Logout completed (token may have already been revoked)" });
    }

    /// <summary>
    /// Retourne les informations d'authentification.
    /// L'authentification se fait exclusivement via Keycloak.
    /// </summary>
    /// <response code="200">Informations sur le système d'authentification.</response>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAuthInfo()
    {
        return Ok(new
        {
            message = "Authentication is handled by the API. Use /api/auth/login to obtain a JWT token.",
            endpoints = new
            {
                login = "/api/auth/login",
                refresh = "/api/auth/refresh",
                logout = "/api/auth/logout",
                me = "/api/auth/me"
            }
        });
    }

    /// <summary>
    /// Retourne les informations de l'utilisateur actuellement authentifié.
    /// </summary>
    /// <response code="200">Informations utilisateur.</response>
    /// <response code="401">Non authentifié.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var username = User.FindFirst("preferred_username")?.Value;
        var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value;
        var lastName = User.FindFirst(ClaimTypes.Surname)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            keycloakUserId = userId,
            email,
            username,
            firstName,
            lastName,
            roles
        });
    }
}

