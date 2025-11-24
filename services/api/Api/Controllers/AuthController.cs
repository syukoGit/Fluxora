namespace Api.Controllers;

using System.Security.Claims;
using Api.DTOs.Auth;
using Api.Services.Keycloak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <inheritdoc />
/// <summary>
/// Authentication controller.
/// Proxies calls to Keycloak to hide the underlying infrastructure.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IKeycloakService keycloakService, ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="request">The user's credentials.</param>
    /// <response code="200">Authentication successful, token returned.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">Incorrect credentials.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(KeycloakTokenResponseDto), StatusCodes.Status200OK)]
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
            var tokenResponse = await keycloakService.LoginAsync(request.Username, request.Password);
            return Ok(tokenResponse);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Login failed for user: {Username}", request.Username);
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Authentication service error for user: {Username}", request.Username);
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
    [ProducesResponseType(typeof(KeycloakTokenResponseDto), StatusCodes.Status201Created)]
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
            try
            {
                bool success = await keycloakService.RegisterAsync(request.UserName, request.Password, request.Email,
                                                                   request.FirstName, request.LastName);

                if (!success)
                {
                    return BadRequest(new { message = "Failed to register user." });
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "Registration failed for user: {UserName}", request.UserName);

                return BadRequest(new { message = ex.Message });
            }

            // 2. Authentifier automatiquement l'utilisateur
            try
            {
                var tokenResponse = await keycloakService.LoginAsync(request.UserName, request.Password);
                logger.LogInformation("User registered and auto-logged in: {UserName}", request.UserName);

                return CreatedAtAction(nameof(GetCurrentUser), null, tokenResponse);
            }
            catch (Exception loginEx)
            {
                // Si l'auto-login échoue, l'utilisateur est créé mais devra se logger manuellement
                logger.LogWarning(loginEx, "User registered but auto-login failed for: {UserName}", request.UserName);

                return StatusCode(StatusCodes.Status201Created,
                                  new
                                  {
                                      message = "User registered successfully. Please log in.",
                                      userName = request.UserName,
                                  });
            }
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error during registration for user: {UserName}", request.UserName);

            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                              new { message = "Authentication service is unavailable." });
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
    [ProducesResponseType(typeof(KeycloakTokenResponseDto), StatusCodes.Status200OK)]
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
            var tokenResponse = await keycloakService.RefreshTokenAsync(request.RefreshToken);
            return Ok(tokenResponse);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Token refresh failed");
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Authentication service error during token refresh");
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

        bool success = await keycloakService.LogoutAsync(request.RefreshToken);

        return Ok(success
                      ? new { message = "Logout successful" }
                      : new { message = "Logout completed (token may have already been revoked)" });
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
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string? email = User.FindFirst(ClaimTypes.Email)?.Value;
        string? username = User.FindFirst("preferred_username")?.Value;
        string? firstName = User.FindFirst(ClaimTypes.GivenName)?.Value;
        string? lastName = User.FindFirst(ClaimTypes.Surname)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        return Ok(new
        {
            keycloakUserId = userId,
            email,
            username,
            firstName,
            lastName,
            roles,
        });
    }
}

