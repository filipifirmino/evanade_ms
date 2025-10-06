using Microsoft.Extensions.Configuration;
using APIGateway.ApplicationCore.Domain.Entities;
using APIGateway.Infra.RateLimiting;
using Xunit;

namespace APIGateway.Tests.Infra.Tests.RateLimiting;

public class InMemoryRateLimitServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly InMemoryRateLimitService _rateLimitService;

    public InMemoryRateLimitServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _rateLimitService = new InMemoryRateLimitService(_configurationMock.Object);
    }

    [Fact]
    public async Task AllowRequestAsync_WithDefaultPolicy_ShouldAllowRequest()
    {
        var category = "test";
        var clientId = "client1";

        var result = await _rateLimitService.AllowRequestAsync(category, clientId);

        Assert.True(result);
    }

    [Fact]
    public async Task AllowRequestAsync_WithMultipleRequestsWithinLimit_ShouldAllowAll()
    {
        var category = "test";
        var clientId = "client1";

        var results = new List<bool>();
        for (int i = 0; i < 5; i++)
        {
            results.Add(await _rateLimitService.AllowRequestAsync(category, clientId));
        }

        Assert.All(results, r => Assert.True(r));
    }

    [Fact]
    public async Task AllowRequestAsync_WithExceedingLimit_ShouldDenyRequest()
    {
        var category = "auth";
        var clientId = "client1";

        var results = new List<bool>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(await _rateLimitService.AllowRequestAsync(category, clientId));
        }

        Assert.Equal(5, results.Count(r => r));
        Assert.Equal(5, results.Count(r => !r));
    }

    [Fact]
    public async Task AllowRequestAsync_WithDifferentClients_ShouldTrackSeparately()
    {
        var category = "auth";
        var client1 = "client1";
        var client2 = "client2";

        var client1Results = new List<bool>();
        var client2Results = new List<bool>();

        for (int i = 0; i < 6; i++)
        {
            client1Results.Add(await _rateLimitService.AllowRequestAsync(category, client1));
            client2Results.Add(await _rateLimitService.AllowRequestAsync(category, client2));
        }

        Assert.Equal(5, client1Results.Count(r => r));
        Assert.Equal(1, client1Results.Count(r => !r));
        Assert.Equal(5, client2Results.Count(r => r));
        Assert.Equal(1, client2Results.Count(r => !r));
    }

    [Fact]
    public async Task GetRateLimitInfoAsync_WithNoRequests_ShouldReturnFullLimit()
    {
        var clientId = "client1";
        var endpoint = "test";

        var info = await _rateLimitService.GetRateLimitInfoAsync(clientId, endpoint);

        Assert.NotNull(info);
        Assert.Equal("test", info.Endpoint);
        Assert.Equal(clientId, info.ClientId);
        Assert.Equal(30, info.Limit);
        Assert.Equal(30, info.Remaining);
    }

    [Fact]
    public async Task GetRateLimitInfoAsync_WithSomeRequests_ShouldReturnCorrectRemaining()
    {
        var clientId = "client1";
        var endpoint = "auth";

        await _rateLimitService.AllowRequestAsync(endpoint, clientId);
        await _rateLimitService.AllowRequestAsync(endpoint, clientId);

        var info = await _rateLimitService.GetRateLimitInfoAsync(clientId, endpoint);

        Assert.NotNull(info);
        Assert.Equal("auth", info.Endpoint);
        Assert.Equal(clientId, info.ClientId);
        Assert.Equal(5, info.Limit);
        Assert.Equal(3, info.Remaining);
    }


    [Theory]
    [InlineData("auth", 5, 60)]
    [InlineData("inventory", 100, 60)]
    [InlineData("sales", 50, 60)]
    [InlineData("unknown", 30, 60)]
    public void GetPolicyForEndpoint_WithDifferentEndpoints_ShouldReturnCorrectDefaultPolicy(string endpoint, int expectedMaxRequests, int expectedWindowSeconds)
    {
        var section = new Mock<IConfigurationSection>();
        section.Setup(x => x.GetChildren()).Returns(new List<IConfigurationSection>());

        _configurationMock.Setup(x => x.GetSection($"RateLimit:{endpoint}"))
            .Returns(section.Object);

        var policy = _rateLimitService.GetPolicyForEndpoint(endpoint);

        Assert.NotNull(policy);
        Assert.Equal(endpoint, policy.Endpoint);
        Assert.Equal(expectedMaxRequests, policy.MaxRequests);
        Assert.Equal(expectedWindowSeconds, policy.WindowSeconds);
    }

}
