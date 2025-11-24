namespace Api.Tests.Controllers.AuthControllerTests;

using Api.DTOs.Auth;
using Api.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <inheritdoc />
/// <summary>
/// Tests for POST /api/auth/login endpoint.
/// </summary>
public class LoginTests : AuthControllerTestBase
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        (string username, string password) = AuthFixture.ValidCredentials;
        var request = new LoginRequestDto { Username = username, Password = password };
        var expectedToken = AuthFixture.ValidToken;

        MockKeycloakService.LoginAsync(request.Username, request.Password).Returns(Task.FromResult(expectedToken));

        // Act
        var result = await Controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult.Value.Should().BeEquivalentTo(expectedToken);

        await MockKeycloakService.Received(1).LoginAsync(request.Username, request.Password);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        (string username, string password) = AuthFixture.InvalidCredentials;
        var request = new LoginRequestDto { Username = username, Password = password };

        MockKeycloakService.LoginAsync(request.Username, request.Password)
                           .Returns<KeycloakTokenResponseDto>(_ => throw new UnauthorizedAccessException(
                                                                       "Invalid credentials"));

        // Act
        var result = await Controller.Login(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult.Should().NotBeNull();
        unauthorizedResult.Value.Should().NotBeNull();

        await MockKeycloakService.Received(1).LoginAsync(request.Username, request.Password);
    }

    [Fact]
    public async Task Login_ServiceUnavailable_ReturnsServiceUnavailable()
    {
        // Arrange
        (string username, string password) = AuthFixture.ValidCredentials;
        var request = new LoginRequestDto { Username = username, Password = password };

        MockKeycloakService.LoginAsync(request.Username, request.Password)
                           .Returns<KeycloakTokenResponseDto>(_ => throw new InvalidOperationException(
                                                                       "Service unavailable"));

        // Act
        var result = await Controller.Login(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);

        await MockKeycloakService.Received(1).LoginAsync(request.Username, request.Password);
    }

    [Fact]
    public async Task Login_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        (string username, string password) = AuthFixture.InvalidCredentials;
        var request = new LoginRequestDto { Username = username, Password = password };

        Controller.ModelState.AddModelError("Username", "The Username field is required.");
        Controller.ModelState.AddModelError("Password", "The Password field is required.");

        // Act
        var result = await Controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.Value.Should().NotBeNull();

        await MockKeycloakService.DidNotReceive().LoginAsync(Arg.Any<string>(), Arg.Any<string>());
    }
}
