namespace Api.Tests.Fixtures;

using Api.DTOs.Auth;

public class AuthFixture
{
    public const string UserName = "testuser";

    public const string UserEmail = "testuser@example.com";

    public static (string, string) ValidCredentials => (UserName, "testpassword");

    public static (string, string) InvalidCredentials => (UserName, "wrongpassword");

    public static KeycloakTokenResponseDto ValidToken => new()
    {
        AccessToken = "valid_access_token",
        RefreshToken = "valid_refresh_token",
        ExpiresIn = 3600,
        RefreshExpiresIn = 7200,
        TokenType = "Bearer",
    };
}