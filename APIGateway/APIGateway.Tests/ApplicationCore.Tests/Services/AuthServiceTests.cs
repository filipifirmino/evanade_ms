using AutoBogus;
using Microsoft.Extensions.Logging;
using Moq;
using APIGateway.ApplicationCore.Abstractions;
using APIGateway.ApplicationCore.Domain.Entities;
using APIGateway.ApplicationCore.DTOs;
using APIGateway.ApplicationCore.Exceptions;
using APIGateway.ApplicationCore.Services;
using Xunit;

namespace APIGateway.Tests.ApplicationCore.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _authService = new AuthService(_tokenServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnLoginResponse()
    {
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "admin123"
        };

        var expectedToken = "jwt-token-here";
        var user = new User
        {
            Id = "1",
            Username = "admin",
            Role = "Admin",
            CreatedAt = DateTime.UtcNow
        };

        _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(expectedToken);

        var result = await _authService.AuthenticateAsync(request);

        Assert.NotNull(result);
        Assert.Equal(expectedToken, result.Token);
        Assert.Equal("admin", result.Username);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);

        _tokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Once);
        _loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Information),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Usuário autenticado com sucesso: {request.Username}")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidCredentials_ShouldReturnNull()
    {
        var request = new LoginRequest
        {
            Username = "invaliduser",
            Password = "wrongpassword"
        };

        var result = await _authService.AuthenticateAsync(request);

        Assert.Null(result);
        _tokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
        _loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Warning),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Credenciais inválidas para usuário: {request.Username}")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithUserCredentials_ShouldReturnLoginResponse()
    {
        var request = new LoginRequest
        {
            Username = "user",
            Password = "user123"
        };

        var expectedToken = "jwt-token-user";
        var user = new User
        {
            Id = "2",
            Username = "user",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(expectedToken);

        var result = await _authService.AuthenticateAsync(request);

        Assert.NotNull(result);
        Assert.Equal(expectedToken, result.Token);
        Assert.Equal("user", result.Username);
        Assert.Equal(3600, result.ExpiresIn);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenTokenServiceThrowsException_ShouldThrowAuthenticationException()
    {
        var request = new LoginRequest
        {
            Username = "admin",
            Password = "admin123"
        };

        _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Throws(new Exception("Token generation failed"));

        var exception = await Assert.ThrowsAsync<AuthenticationException>(() => _authService.AuthenticateAsync(request));

        Assert.Contains("Erro interno durante autenticação", exception.Message);
        _loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Error),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Erro durante autenticação do usuário: {request.Username}")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ShouldReturnUser()
    {
        var token = "valid-jwt-token";
        var expectedUser = new User
        {
            Id = "1",
            Username = "admin",
            Role = "Admin"
        };

        _tokenServiceMock.Setup(x => x.ValidateTokenAsync(token))
            .ReturnsAsync(expectedUser);

        var result = await _authService.ValidateTokenAsync(token);

        Assert.NotNull(result);
        Assert.Equal(expectedUser.Id, result.Id);
        Assert.Equal(expectedUser.Username, result.Username);
        Assert.Equal(expectedUser.Role, result.Role);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ShouldReturnNull()
    {
        var token = "invalid-jwt-token";

        _tokenServiceMock.Setup(x => x.ValidateTokenAsync(token))
            .ThrowsAsync(new Exception("Invalid token"));

        var result = await _authService.ValidateTokenAsync(token);

        Assert.Null(result);
        _loggerMock.Verify(x => x.Log(
            It.Is<LogLevel>(l => l == LogLevel.Error),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro durante validação do token")),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Theory]
    [InlineData("admin", "admin123", true)]
    [InlineData("user", "user123", true)]
    [InlineData("invalid", "password", false)]
    [InlineData("admin", "wrong", false)]
    [InlineData("", "password", false)]
    [InlineData("user", "", false)]
    [InlineData(null, "password", false)]
    [InlineData("user", null, false)]
    public void ValidateCredentials_WithDifferentInputs_ShouldReturnExpectedResult(string username, string password, bool expected)
    {
        var result = _authService.ValidateCredentials(username, password);

        Assert.Equal(expected, result);
    }
}
