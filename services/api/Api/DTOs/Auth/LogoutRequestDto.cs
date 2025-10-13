namespace Api.DTOs.Auth;

/// <summary>
/// Logout request.
/// </summary>
public class LogoutRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
