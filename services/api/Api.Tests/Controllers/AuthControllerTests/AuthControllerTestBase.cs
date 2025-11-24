namespace Api.Tests.Controllers.AuthControllerTests;

using Api.Controllers;
using Api.Services.Keycloak;
using Microsoft.Extensions.Logging;

/// <inheritdoc />
/// <summary>
/// Base fixture for all auth controller tests.
/// </summary>
public abstract class AuthControllerTestBase : IDisposable
{
    protected readonly IKeycloakService MockKeycloakService;

    protected readonly AuthController Controller;

    protected AuthControllerTestBase()
    {
        MockKeycloakService = Substitute.For<IKeycloakService>();
        var mockLogger = Substitute.For<ILogger<AuthController>>();
        Controller = new AuthController(MockKeycloakService, mockLogger);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
