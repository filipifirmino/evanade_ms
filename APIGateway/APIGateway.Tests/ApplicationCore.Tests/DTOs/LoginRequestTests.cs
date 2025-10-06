using APIGateway.ApplicationCore.DTOs;
using Xunit;

namespace APIGateway.Tests.ApplicationCore.Tests.DTOs;

public class LoginRequestTests
{
    [Fact]
    public void IsValid_WithValidUsernameAndPassword_ShouldReturnTrue()
    {
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "password123"
        };

        var result = request.IsValid();

        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithEmptyUsername_ShouldReturnFalse()
    {
        var request = new LoginRequest
        {
            Username = "",
            Password = "password123"
        };

        var result = request.IsValid();

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithNullUsername_ShouldReturnFalse()
    {
        var request = new LoginRequest
        {
            Username = null,
            Password = "password123"
        };

        var result = request.IsValid();

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithEmptyPassword_ShouldReturnFalse()
    {
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = ""
        };

        var result = request.IsValid();

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithNullPassword_ShouldReturnFalse()
    {
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = null
        };

        var result = request.IsValid();

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithWhitespaceUsername_ShouldReturnFalse()
    {
        var request = new LoginRequest
        {
            Username = "   ",
            Password = "password123"
        };

        var result = request.IsValid();

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithWhitespacePassword_ShouldReturnFalse()
    {
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "   "
        };

        var result = request.IsValid();

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithBothEmpty_ShouldReturnFalse()
    {
        var request = new LoginRequest
        {
            Username = "",
            Password = ""
        };

        var result = request.IsValid();

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithBothNull_ShouldReturnFalse()
    {
        var request = new LoginRequest
        {
            Username = null,
            Password = null
        };

        var result = request.IsValid();

        Assert.False(result);
    }
}
