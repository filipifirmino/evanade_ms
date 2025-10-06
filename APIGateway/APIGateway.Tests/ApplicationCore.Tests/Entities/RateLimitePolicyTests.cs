using APIGateway.ApplicationCore.Domain.Entities;
using Xunit;

namespace APIGateway.Tests.ApplicationCore.Tests.Entities;

public class RateLimitePolicyTests
{
    [Fact]
    public void IsWithinLimit_WithCurrentRequestsLessThanMax_ShouldReturnTrue()
    {
        var policy = new RateLimitePolicy
        {
            Endpoint = "auth",
            MaxRequests = 10,
            WindowSeconds = 60
        };

        var result = policy.IsWithinLimit(5);

        Assert.True(result);
    }

    [Fact]
    public void IsWithinLimit_WithCurrentRequestsEqualToMax_ShouldReturnTrue()
    {
        var policy = new RateLimitePolicy
        {
            Endpoint = "auth",
            MaxRequests = 10,
            WindowSeconds = 60
        };

        var result = policy.IsWithinLimit(10);

        Assert.True(result);
    }

    [Fact]
    public void IsWithinLimit_WithCurrentRequestsGreaterThanMax_ShouldReturnFalse()
    {
        var policy = new RateLimitePolicy
        {
            Endpoint = "auth",
            MaxRequests = 10,
            WindowSeconds = 60
        };

        var result = policy.IsWithinLimit(15);

        Assert.False(result);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(5, true)]
    [InlineData(10, true)]
    [InlineData(11, false)]
    [InlineData(20, false)]
    public void IsWithinLimit_WithDifferentValues_ShouldReturnExpectedResult(int currentRequests, bool expected)
    {
        var policy = new RateLimitePolicy
        {
            Endpoint = "test",
            MaxRequests = 10,
            WindowSeconds = 60
        };

        var result = policy.IsWithinLimit(currentRequests);

        Assert.Equal(expected, result);
    }
}
