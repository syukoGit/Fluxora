namespace Api.DTOs.Auth;

/// <summary>
/// Token refresh request.
/// </summary>
public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}