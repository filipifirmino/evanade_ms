using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using APIGateway.ApplicationCore.Abstractions;
using APIGateway.ApplicationCore.DTOs;
using APIGateway.Web.Controllers;
using Xunit;

namespace APIGateway.Tests.Web.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _authController;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _authController = new AuthController(_authServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Login_WithValidRequest_ShouldReturnOkResult()
    {
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "admin123"
        };

        var loginResponse = new LoginResponse
        {
            Token = "jwt-token-here",
            Username = "admin",
            ExpiresIn = 3600,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };

        _authServiceMock.Setup(x => x.AuthenticateAsync(request))
            .ReturnsAsync(loginResponse);

        var result = await _authController.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal(loginResponse.Token, response.Token);
        Assert.Equal(loginResponse.Username, response.Username);
        Assert.Equal(loginResponse.ExpiresIn, response.ExpiresIn);

        _authServiceMock.Verify(x => x.AuthenticateAsync(request), Times.Once);
        _loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"User {request.Username} logged in successfully")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidRequest_ShouldReturnBadRequest()
    {
        var request = new LoginRequest
        {
            Username = "",
            Password = "password123"
        };

        var result = await _authController.Login(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = badRequestResult.Value;
        Assert.NotNull(response);

        _authServiceMock.Verify(x => x.AuthenticateAsync(It.IsAny<LoginRequest>()), Times.Never);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        var request = new LoginRequest
        {
            Username = "invaliduser",
            Password = "wrongpassword"
        };

        _authServiceMock.Setup(x => x.AuthenticateAsync(request))
            .ReturnsAsync((LoginResponse)null);

        var result = await _authController.Login(request);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var response = unauthorizedResult.Value;
        Assert.NotNull(response);

        _authServiceMock.Verify(x => x.AuthenticateAsync(request), Times.Once);
        _loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Warning),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Failed login attempt for user: {request.Username}")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Login_WhenAuthServiceThrowsException_ShouldReturnInternalServerError()
    {
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "admin123"
        };

        _authServiceMock.Setup(x => x.AuthenticateAsync(request))
            .ThrowsAsync(new Exception("Service error"));

        var result = await _authController.Login(request);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);

        _authServiceMock.Verify(x => x.AuthenticateAsync(request), Times.Once);
        _loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Error),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during login")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }


    [Theory]
    [InlineData("admin", "admin123")]
    [InlineData("user", "user123")]
    public async Task Login_WithDifferentValidCredentials_ShouldReturnOkResult(string username, string password)
    {
        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        var loginResponse = new LoginResponse
        {
            Token = $"jwt-token-{username}",
            Username = username,
            ExpiresIn = 3600,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        };

        _authServiceMock.Setup(x => x.AuthenticateAsync(request))
            .ReturnsAsync(loginResponse);

        var result = await _authController.Login(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal(loginResponse.Token, response.Token);
        Assert.Equal(loginResponse.Username, response.Username);
    }
}
