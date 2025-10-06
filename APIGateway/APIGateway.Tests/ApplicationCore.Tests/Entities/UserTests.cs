using APIGateway.ApplicationCore.Domain.Entities;
using Xunit;

namespace APIGateway.Tests.ApplicationCore.Tests.Entities;

public class UserTests
{
    [Fact]
    public void IsValid_WithValidUsername_ShouldReturnTrue()
    {
        var user = new User
        {
            Id = "1",
            Username = "testuser",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        var result = user.IsValid();

        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithEmptyUsername_ShouldReturnFalse()
    {
        var user = new User
        {
            Id = "1",
            Username = "",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        var result = user.IsValid();

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithNullUsername_ShouldReturnFalse()
    {
        var user = new User
        {
            Id = "1",
            Username = null,
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        var result = user.IsValid();

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithWhitespaceUsername_ShouldReturnFalse()
    {
        var user = new User
        {
            Id = "1",
            Username = "   ",
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        var result = user.IsValid();

        Assert.False(result);
    }

    [Theory]
    [InlineData("admin", "Admin")]
    [InlineData("user", "User")]
    [InlineData("guest", "Guest")]
    public void IsValid_WithDifferentRoles_ShouldReturnTrue(string username, string role)
    {
        var user = new User
        {
            Id = "1",
            Username = username,
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        var result = user.IsValid();

        Assert.True(result);
    }
}
