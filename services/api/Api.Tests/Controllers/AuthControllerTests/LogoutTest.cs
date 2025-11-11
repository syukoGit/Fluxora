namespace Api.Tests.Controllers.AuthControllerTests;

using Api.DTOs.Auth;
using Api.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;

public class LogoutTest : AuthControllerTestBase
{
    [Fact]
    public async Task Logout_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        string validRefreshToken = AuthFixture.ValidToken.RefreshToken;

        MockKeycloakService.LogoutAsync(validRefreshToken).Returns(Task.FromResult(true));

        // Act
        var result = await Controller.Logout(new LogoutRequestDto { RefreshToken = validRefreshToken });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.Value.Should().BeEquivalentTo(new { message = "Logout successful" });

        await MockKeycloakService.Received(1).LogoutAsync(validRefreshToken);
    }

    [Fact]
    public async Task Logout_AlreadyRevokedToken_ReturnsNoContent()
    {
        // Arrange
        string revokedRefreshToken = AuthFixture.ValidToken.RefreshToken;

        MockKeycloakService.LogoutAsync(revokedRefreshToken).Returns(Task.FromResult(false));

        // Act
        var result = await Controller.Logout(new LogoutRequestDto { RefreshToken = revokedRefreshToken });

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();

        okResult.Value.Should()
                .BeEquivalentTo(new { message = "Logout completed (token may have already been revoked)" });

        await MockKeycloakService.Received(1).LogoutAsync(revokedRefreshToken);
    }

    [Fact]
    public async Task Logout_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var request = new LogoutRequestDto { RefreshToken = string.Empty };

        Controller.ModelState.AddModelError("RefreshToken", "The RefreshToken field is required.");

        // Act
        var result = await Controller.Logout(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.Value.Should().BeOfType<SerializableError>();

        await MockKeycloakService.DidNotReceive().LogoutAsync(Arg.Any<string>());
    }
}