using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using APIGateway.ApplicationCore.Domain.Entities;
using APIGateway.Infra.Authentication;
using Xunit;

namespace APIGateway.Tests.Infra.Tests.Authentication;

public class JwtTokenServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly JwtTokenService _jwtTokenService;

    public JwtTokenServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _jwtTokenService = new JwtTokenService(_configurationMock.Object);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ShouldReturnValidToken()
    {
        var user = new User
        {
            Id = "1",
            Username = "testuser",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        SetupConfiguration();

        var token = _jwtTokenService.GenerateToken(user);

        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.Equal("TestIssuer", jwtToken.Issuer);
        Assert.Equal("TestAudience", jwtToken.Audiences.First());
        Assert.Contains(jwtToken.Claims, c => c.Type == "userId" && c.Value == "1");
        Assert.Contains(jwtToken.Claims, c => c.Type == "username" && c.Value == "testuser");
        Assert.Contains(jwtToken.Claims, c => c.Type == "role" && c.Value == "User");
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ShouldReturnUser()
    {
        var user = new User
        {
            Id = "1",
            Username = "testuser",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        SetupConfiguration();

        var token = _jwtTokenService.GenerateToken(user);
        var result = await _jwtTokenService.ValidateTokenAsync(token);

        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
        Assert.Equal("testuser", result.Username);
        Assert.Equal("User", result.Role);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ShouldReturnNull()
    {
        SetupConfiguration();

        var invalidToken = "invalid.token.here";
        var result = await _jwtTokenService.ValidateTokenAsync(invalidToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithExpiredToken_ShouldReturnNull()
    {
        // Criar um token manualmente que já está expirado
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("ThisIsAVeryLongSecretKeyForTestingPurposesOnly");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("userId", "1"),
                new Claim("username", "testuser"),
                new Claim(ClaimTypes.Role, "User")
            }),
            NotBefore = DateTime.UtcNow.AddMinutes(-2), // Token válido há 2 minutos
            Expires = DateTime.UtcNow.AddMinutes(-1), // Token expirado há 1 minuto
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var expiredToken = tokenHandler.WriteToken(token);
        
        var result = await _jwtTokenService.ValidateTokenAsync(expiredToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithWrongIssuer_ShouldReturnNull()
    {
        var user = new User
        {
            Id = "1",
            Username = "testuser",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        SetupConfiguration();

        var token = _jwtTokenService.GenerateToken(user);

        SetupConfiguration(issuer: "WrongIssuer");

        var result = await _jwtTokenService.ValidateTokenAsync(token);

        Assert.Null(result);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithWrongAudience_ShouldReturnNull()
    {
        var user = new User
        {
            Id = "1",
            Username = "testuser",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        SetupConfiguration();

        var token = _jwtTokenService.GenerateToken(user);

        SetupConfiguration(audience: "WrongAudience");

        var result = await _jwtTokenService.ValidateTokenAsync(token);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("Admin", "Admin")]
    [InlineData("User", "User")]
    [InlineData("Guest", "Guest")]
    public void GenerateToken_WithDifferentRoles_ShouldIncludeCorrectRole(string role, string expectedRole)
    {
        var user = new User
        {
            Id = "1",
            Username = "testuser",
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        SetupConfiguration();

        var token = _jwtTokenService.GenerateToken(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        Assert.Contains(jwtToken.Claims, c => c.Type == "role" && c.Value == expectedRole);
    }

    private void SetupConfiguration(string issuer = "TestIssuer", string audience = "TestAudience", int expirationMinutes = 60)
    {
        _configurationMock.Setup(x => x["Jwt:SecretKey"]).Returns("ThisIsAVeryLongSecretKeyForTestingPurposesOnly");
        _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns(issuer);
        _configurationMock.Setup(x => x["Jwt:Audience"]).Returns(audience);
        _configurationMock.Setup(x => x["Jwt:ExpirationMinutes"]).Returns(expirationMinutes.ToString());
    }
}
