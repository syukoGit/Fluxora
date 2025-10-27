namespace Api.Tests.Controllers.AuthControllerTests;

using Api.DTOs.Auth;
using Api.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

/// <inheritdoc />
/// <summary>
/// Tests for POST /api/auth/register endpoint.
/// </summary>
public class RegisterTests : AuthControllerTestBase
{
    [Fact]
    public async Task Register_ValidRequest_ReturnsCreatedWithToken()
    {
        // Arrange
        (string username, string password) = AuthFixture.ValidCredentials;

        var request = new RegisterRequest
        {
            UserName = username, Password = password, Email = AuthFixture.UserEmail,
        };

        MockKeycloakService.RegisterAsync(request.UserName, request.Password, request.Email, request.FirstName,
                                          request.LastName)
                           .Returns(Task.FromResult(true));

        MockKeycloakService.LoginAsync(request.UserName, request.Password)
                           .Returns(Task.FromResult(AuthFixture.ValidToken));

        // Act
        var result = await Controller.Register(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult.Should().NotBeNull();
        createdResult.Value.Should().BeEquivalentTo(AuthFixture.ValidToken);

        await MockKeycloakService.Received(1)
                                 .RegisterAsync(request.UserName, request.Password, request.Email, request.FirstName,
                                                request.LastName);
    }

    [Fact]
    public async Task Register_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        (string username, string password) = AuthFixture.InvalidCredentials;

        var request = new RegisterRequest
        {
            UserName = username, Password = password, Email = AuthFixture.UserEmail,
        };

        Controller.ModelState.AddModelError("UserName", "The UserName field is required.");
        Controller.ModelState.AddModelError("Password", "The Password field is required.");
        Controller.ModelState.AddModelError("Email", "The Email field is not a valid e-mail address.");

        // Act
        var result = await Controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.Value.Should().NotBeNull();

        await MockKeycloakService.DidNotReceive()
                                 .RegisterAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                                                Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Register_RegistrationFails_ReturnsBadRequest()
    {
        // Arrange
        (string username, string password) = AuthFixture.InvalidCredentials;

        var request = new RegisterRequest
        {
            UserName = username, Password = password, Email = AuthFixture.UserEmail,
        };

        MockKeycloakService.RegisterAsync(request.UserName, request.Password, request.Email, request.FirstName,
                                          request.LastName)
                           .Returns(Task.FromResult(false));

        // Act
        var result = await Controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.Value.Should().NotBeNull();

        await MockKeycloakService.Received(1)
                                 .RegisterAsync(request.UserName, request.Password, request.Email, request.FirstName,
                                                request.LastName);
    }

    [Fact]
    public async Task Register_RegistrationFails_ReturnsBadRequestDueToInvalidOperation()
    {
        // Arrange
        (string username, string password) = AuthFixture.ValidCredentials;

        var request = new RegisterRequest
        {
            UserName = username, Password = password, Email = AuthFixture.UserEmail,
        };

        MockKeycloakService
            .RegisterAsync(request.UserName, request.Password, request.Email, request.FirstName, request.LastName)
            .Returns<Task<bool>>(_ => throw new InvalidOperationException("User already exists"));

        // Act
        var result = await Controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult.Value.Should().NotBeNull();

        await MockKeycloakService.Received(1)
                                 .RegisterAsync(request.UserName, request.Password, request.Email, request.FirstName,
                                                request.LastName);
    }

    [Fact]
    public async Task Register_ServiceUnavailableDuringRegistration_ReturnsServiceUnavailable()
    {
        // Arrange
        (string username, string password) = AuthFixture.ValidCredentials;

        var request = new RegisterRequest
        {
            UserName = username, Password = password, Email = AuthFixture.UserEmail,
        };

        MockKeycloakService
            .RegisterAsync(request.UserName, request.Password, request.Email, request.FirstName, request.LastName)
            .Returns<Task<bool>>(_ => throw new HttpRequestException("Service unavailable"));

        // Act
        var result = await Controller.Register(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);

        await MockKeycloakService.Received(1)
                                 .RegisterAsync(request.UserName, request.Password, request.Email, request.FirstName,
                                                request.LastName);
    }

    [Fact]
    public async Task Register_AutoLoginFails_ReturnsCreatedWithMessage()
    {
        // Arrange
        (string username, string password) = AuthFixture.ValidCredentials;

        var request = new RegisterRequest
        {
            UserName = username, Password = password, Email = AuthFixture.UserEmail,
        };

        MockKeycloakService.RegisterAsync(request.UserName, request.Password, request.Email, request.FirstName,
                                          request.LastName)
                           .Returns(Task.FromResult(true));

        MockKeycloakService.LoginAsync(request.UserName, request.Password)
                           .Returns<Task<KeycloakTokenResponseDto>>(_ => throw new Exception("Auto-login failed"));

        // Act
        var result = await Controller.Register(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        objectResult.Value.Should().NotBeNull();

        await MockKeycloakService.Received(1)
                                 .RegisterAsync(request.UserName, request.Password, request.Email, request.FirstName,
                                                request.LastName);
    }
}
