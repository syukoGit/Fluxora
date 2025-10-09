namespace Api.DTOs.Auth;

/// <summary>
/// Réponse d'authentification standard contenant les tokens et les infos utilisateur.
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new();
}
