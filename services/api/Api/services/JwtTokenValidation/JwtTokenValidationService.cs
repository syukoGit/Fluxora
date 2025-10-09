namespace Api.Services.JwtTokenValidation;

using System.Security.Claims;
using Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service responsible for JWT token validation and user provisioning from Keycloak
/// </summary>
public class JwtTokenValidationService(
    ILogger<JwtTokenValidationService> logger,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager) : IJwtTokenValidationService
{
    private readonly ILogger<JwtTokenValidationService> _logger = logger;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;

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

            // Retrieve or create the user
            var user = await GetOrCreateUserAsync(keycloakUserId, email, preferredUsername, firstName, lastName);

            if (user == null)
            {
                context.Fail("Failed to provision user");
                return;
            }

            // Verify that the user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("Inactive user {UserId} attempted to authenticate", user.Id);
                context.Fail("User account is inactive");
                return;
            }

            // Update last login date
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Synchronize roles from Keycloak
            await SynchronizeUserRolesAsync(user, context.Principal);

            _logger.LogInformation("User {UserId} successfully provisioned/synced from Keycloak", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user provisioning from Keycloak");
            context.Fail($"User provisioning error: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves an existing user or creates a new one from Keycloak information
    /// </summary>
    private async Task<ApplicationUser?> GetOrCreateUserAsync(
        string keycloakUserId,
        string? email,
        string? preferredUsername,
        string? firstName,
        string? lastName)
    {
        // Look for user by KeycloakUserId
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.KeycloakUserId == keycloakUserId);

        if (user == null && !string.IsNullOrEmpty(email))
        {
            // Search by email (case of an account created before Keycloak migration)
            user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                // Link existing account to Keycloak
                user.KeycloakUserId = keycloakUserId;
                _logger.LogInformation("Linking existing user {UserId} to Keycloak ID {KeycloakUserId}", user.Id, keycloakUserId);
            }
        }

        if (user == null)
        {
            // Create a new user account provisioned from Keycloak
            user = new ApplicationUser
            {
                KeycloakUserId = keycloakUserId,
                UserName = preferredUsername ?? email ?? keycloakUserId,
                Email = email,
                EmailConfirmed = true, // Trust Keycloak
                FirstName = firstName,
                LastName = lastName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to create user from Keycloak: {Errors}", errors);
                return null;
            }

            _logger.LogInformation("Created new user {UserId} from Keycloak ID {KeycloakUserId}", user.Id, keycloakUserId);
        }
        else
        {
            // Update existing user information
            await UpdateUserInfoAsync(user, email, firstName, lastName);
        }

        return user;
    }

    /// <summary>
    /// Updates existing user information
    /// </summary>
    private async Task UpdateUserInfoAsync(ApplicationUser user, string? email, string? firstName, string? lastName)
    {
        var needsUpdate = false;

        if (user.Email != email && !string.IsNullOrEmpty(email))
        {
            user.Email = email;
            needsUpdate = true;
        }

        if (user.FirstName != firstName && !string.IsNullOrEmpty(firstName))
        {
            user.FirstName = firstName;
            needsUpdate = true;
        }

        if (user.LastName != lastName && !string.IsNullOrEmpty(lastName))
        {
            user.LastName = lastName;
            needsUpdate = true;
        }

        if (needsUpdate)
        {
            await _userManager.UpdateAsync(user);
            _logger.LogInformation("Updated user info for {UserId}", user.Id);
        }
    }

    /// <summary>
    /// Synchronizes user roles with those present in the Keycloak token
    /// </summary>
    private async Task SynchronizeUserRolesAsync(ApplicationUser user, ClaimsPrincipal? principal)
    {
        // Extract roles from Keycloak token
        var keycloakRoles = principal?.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .Where(r => !string.IsNullOrEmpty(r))
            .ToList() ?? [];

        // Create missing roles and assign them to the user
        foreach (var roleName in keycloakRoles)
        {
            // Ensure the role exists in the system
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                _logger.LogInformation("Created role {RoleName} from Keycloak", roleName);
            }

            // Assign the role to the user if they don't have it already
            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                await _userManager.AddToRoleAsync(user, roleName);
                _logger.LogInformation("Assigned role {RoleName} to user {UserId}", roleName, user.Id);
            }
        }

        // Remove roles that the user has locally but not in Keycloak anymore
        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Except(keycloakRoles).ToList();

        if (rolesToRemove.Count != 0)
        {
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            _logger.LogInformation("Removed roles {Roles} from user {UserId}", string.Join(", ", rolesToRemove), user.Id);
        }
    }
}
