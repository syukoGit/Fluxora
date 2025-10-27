namespace Api.Tests.Controllers.AuthControllerTests;

using System.Security.Claims;
using Api.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public class GetCurrentUserTest : AuthControllerTestBase
{
    [Fact]
    public void GetCurrentUser_AuthenticatedUser_ReturnsUserInfo()
    {
        // Arrange
        const string userId = "00000000-0000-0000-0000-000000000001";
        const string email = AuthFixture.UserEmail;
        const string username = AuthFixture.UserName;
        const string firstName = "Joe";
        const string lastName = "Doe";
        string[] roles = ["User"];

        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, userId),
            new (ClaimTypes.Email, email),
            new ("preferred_username", username),
            new (ClaimTypes.GivenName, firstName),
            new (ClaimTypes.Surname, lastName),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) },
        };

        // Act
        var result = Controller.GetCurrentUser();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();

        okResult.Value.Should().BeEquivalentTo(new
        {
            keycloakUserId = userId,
            username,
            email,
            firstName,
            lastName,
            roles,
        });
    }
}