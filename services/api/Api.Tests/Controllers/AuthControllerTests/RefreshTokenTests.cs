namespace Api.Tests.Controllers.AuthControllerTests;

using Api.DTOs.Auth;
using Api.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class RefreshTokenTests : AuthControllerTestBase
{
    [Fact]
    public async Task RefreshToken_ValidRequest_ReturnsNewToken()
    {
        // Arrange
        string validRefreshToken = AuthFixture.ValidToken.RefreshToken;

        MockKeycloakService.RefreshTokenAsync(validRefreshToken).Returns(Task.FromResult(AuthFixture.ValidToken));

        // Act
        var result = await Controller.RefreshToken(new RefreshTokenRequestDto { RefreshToken = validRefreshToken });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.Value.Should().BeEquivalentTo(AuthFixture.ValidToken);

        await MockKeycloakService.Received(1).RefreshTokenAsync(validRefreshToken);
    }

    [Fact]
    public async Task RefreshToken_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        const string invalidRefreshToken = "invalid_refresh_token";

        MockKeycloakService.RefreshTokenAsync(invalidRefreshToken)
                           .Returns<Task<KeycloakTokenResponseDto>>(_ => throw new UnauthorizedAccessException(
                                                                             "Invalid refresh token."));

        // Act
        var result = await Controller.RefreshToken(new RefreshTokenRequestDto { RefreshToken = invalidRefreshToken });

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
        unauthorizedResult.Value.Should().BeEquivalentTo(new { message = "Invalid refresh token." });

        await MockKeycloakService.Received(1).RefreshTokenAsync(invalidRefreshToken);
    }

    [Fact]
    public async Task RefreshToken_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var request = new RefreshTokenRequestDto { RefreshToken = string.Empty };

        Controller.ModelState.AddModelError("RefreshToken", "The RefreshToken field is required.");

        // Act
        var result = await Controller.RefreshToken(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.Value.Should().BeOfType<SerializableError>();

        var errors = badRequestResult.Value as SerializableError;
        errors.Should().ContainKey("RefreshToken");

        await MockKeycloakService.DidNotReceive().RefreshTokenAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task RefreshToken_ServiceUnavailable_ReturnsServiceUnavailable()
    {
        // Arrange
        string refreshToken = AuthFixture.ValidToken.RefreshToken;

        MockKeycloakService.RefreshTokenAsync(refreshToken)
                           .Returns<Task<KeycloakTokenResponseDto>>(_ => throw new InvalidOperationException(
                                                                             "Keycloak service is unavailable."));

        // Act
        var result = await Controller.RefreshToken(new RefreshTokenRequestDto { RefreshToken = refreshToken });

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);

        await MockKeycloakService.Received(1).RefreshTokenAsync(refreshToken);
    }
}