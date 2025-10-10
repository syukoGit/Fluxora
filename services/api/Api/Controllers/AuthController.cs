namespace Api.Controllers;

using System.Security.Claims;
using Api.DTOs.Auth;
using Api.Services.Keycloak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Authentication controller.
/// Proxies calls to Keycloak to hide the underlying infrastructure.
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
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">The user's credentials.</param>
    /// <response code="200">Authentication successful, token returned.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">Incorrect credentials.</response>
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
    /// Registers a new user account and automatically logs them in.
    /// </summary>
    /// <param name="request">The new user registration information.</param>
    /// <response code="201">User registered successfully and authentication token returned.</response>
    /// <response code="400">Invalid request or user already exists.</response>
    /// <response code="503">Service unavailable.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(KeycloakTokenResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // 1. Enregistrer l'utilisateur dans Keycloak
            var success = await _keycloakService.RegisterAsync(
                request.Email,
                request.UserName,
                request.Password,
                request.FirstName,
                request.LastName);

            if (!success)
            {
                return BadRequest(new { message = "Failed to register user." });
            }

            // 2. Authentifier automatiquement l'utilisateur
            try
            {
                var tokenResponse = await _keycloakService.LoginAsync(request.Email, request.Password);
                _logger.LogInformation("User registered and auto-logged in: {Email}", request.Email);

                return StatusCode(StatusCodes.Status201Created, tokenResponse);
            }
            catch (Exception loginEx)
            {
                // Si l'auto-login échoue, l'utilisateur est créé mais devra se logger manuellement
                _logger.LogWarning(loginEx, "User registered but auto-login failed for: {Email}", request.Email);

                return StatusCode(StatusCodes.Status201Created, new
                {
                    message = "User registered successfully. Please log in.",
                    email = request.Email
                });
            }
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Registration failed for user: {Email}", request.Email);

            if (ex.Message.Contains("already exists"))
            {
                return BadRequest(new { message = ex.Message });
            }

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = ex.Message });
        }
    }

    /// <summary>
    /// Refreshes an expired access token using a refresh token.
    /// </summary>
    /// <param name="request">The refresh token.</param>
    /// <response code="200">Token refreshed successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
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
    /// Logs out the user by revoking their refresh token.
    /// </summary>
    /// <param name="request">The refresh token to revoke.</param>
    /// <response code="200">Logout successful.</response>
    /// <response code="400">Invalid request.</response>
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
    /// Returns authentication information.
    /// Authentication is handled exclusively via Keycloak.
    /// </summary>
    /// <response code="200">Information about the authentication system.</response>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAuthInfo()
    {
        return Ok(new
        {
            message = "Authentication is handled by the API. Use /api/auth/login to obtain a JWT token.",
            endpoints = new
            {
                register = "/api/auth/register",
                login = "/api/auth/login",
                refresh = "/api/auth/refresh",
                logout = "/api/auth/logout",
                me = "/api/auth/me"
            }
        });
    }

    /// <summary>
    /// Returns information about the currently authenticated user.
    /// </summary>
    /// <response code="200">User information.</response>
    /// <response code="401">Not authenticated.</response>
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

