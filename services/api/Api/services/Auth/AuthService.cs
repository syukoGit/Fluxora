namespace Api.Services.Auth;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Api.Data;
using Api.DTOs;
using Api.DTOs.Auth;
using Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Authentication service handling registration, login, and tokens
/// </summary>
public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ApplicationDbContext context,
    IConfiguration configuration,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly ApplicationDbContext _context = context;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<AuthService> _logger = logger;

    /// <summary>
    /// Registers a new user
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Attempting registration for email: {Email}", request.Email);

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            throw new InvalidOperationException("A user with this email already exists.");
        }

        // Create the new user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("User creation failed: {Errors}", errors);
            throw new InvalidOperationException($"Error creating user: {errors}");
        }

        // Assign default role
        await _userManager.AddToRoleAsync(user, "User");

        _logger.LogInformation("User created successfully: {UserId}", user.Id);

        // TODO: Synchronize with Keycloak
        // await SyncWithKeycloakAsync(user.Id);

        // Generate tokens
        var authResponse = await GenerateAuthResponseAsync(user);

        return authResponse;
    }

    /// <summary>
    /// Authenticates a user
    /// </summary>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt with unknown email: {Email}", request.Email);
            throw new UnauthorizedAccessException("Incorrect email or password.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt with inactive account: {Email}", request.Email);
            throw new UnauthorizedAccessException("This account is disabled.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                _logger.LogWarning("Account locked out: {Email}", request.Email);
                throw new UnauthorizedAccessException("Account locked due to too many failed attempts.");
            }

            _logger.LogWarning("Incorrect password for: {Email}", request.Email);
            throw new UnauthorizedAccessException("Incorrect email or password.");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Login successful for: {UserId}", user.Id);

        // Generate tokens
        var authResponse = await GenerateAuthResponseAsync(user);

        return authResponse;
    }

    /// <summary>
    /// Renews the access token using a refresh token
    /// </summary>
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        _logger.LogInformation("Token refresh attempt");

        var storedToken = await _context.Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (storedToken == null || !storedToken.IsActive)
        {
            _logger.LogWarning("Invalid or expired refresh token");
            throw new UnauthorizedAccessException("Invalid or expired token.");
        }

        var user = storedToken.User;
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("User associated with token not found or inactive");
            throw new UnauthorizedAccessException("Invalid user.");
        }

        // Revoke the old token
        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;

        // Generate new tokens
        var authResponse = await GenerateAuthResponseAsync(user);

        // Mark the replacement token
        storedToken.ReplacedByToken = authResponse.RefreshToken;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);

        return authResponse;
    }

    /// <summary>
    /// Revokes a token
    /// </summary>
    public async Task<bool> RevokeTokenAsync(string token)
    {
        _logger.LogInformation("Token revocation attempt");

        var storedToken = await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (storedToken == null)
        {
            _logger.LogWarning("Token to revoke not found");
            return false;
        }

        if (storedToken.IsRevoked)
        {
            _logger.LogInformation("Token already revoked");
            return true;
        }

        storedToken.IsRevoked = true;
        storedToken.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Token revoked successfully");

        return true;
    }

    /// <summary>
    /// Synchronizes the user with Keycloak
    /// </summary>
    public async Task SyncWithKeycloakAsync(string userId)
    {
        // TODO: Implement synchronization with Keycloak
        // This method will be implemented when the KeycloakService is created
        _logger.LogInformation("Keycloak synchronization for user: {UserId} (not implemented)", userId);
        await Task.CompletedTask;
    }

    #region Private methods

    /// <summary>
    /// Generates the authentication response with tokens and user information
    /// </summary>
    private async Task<AuthResponse> GenerateAuthResponseAsync(ApplicationUser user)
    {
        var accessToken = await GenerateAccessTokenAsync(user);
        var refreshToken = GenerateRefreshToken();

        // Stocker le refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days validity
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<RefreshToken>().Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        return new AuthResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = roles.ToList()
            }
        };
    }

    /// <summary>
    /// Generates a JWT token
    /// </summary>
    private async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add roles to claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Get secret key from configuration
        var secretKey = _configuration["Jwt:SecretKey"] ?? "VotreCleSuperSecreteQuiDoitEtreLongue123456789!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:ValidIssuer"],
            audience: _configuration["Jwt:ValidAudience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a secure random refresh token
    /// </summary>
    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    #endregion
}
