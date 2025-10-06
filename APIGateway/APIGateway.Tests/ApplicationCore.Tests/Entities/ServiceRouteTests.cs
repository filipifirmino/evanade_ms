using APIGateway.ApplicationCore.Domain.Entities;
using Xunit;

namespace APIGateway.Tests.ApplicationCore.Tests.Entities;

public class ServiceRouteTests
{
    [Fact]
    public void SupportsMethod_WithSupportedMethod_ShouldReturnTrue()
    {
        var route = new ServiceRoute
        {
            ServiceName = "TestService",
            BaseUrl = "http://localhost:5000",
            Template = "/api/test",
            Methods = new List<string> { "GET", "POST", "PUT" }
        };

        var result = route.SupportsMethod("GET");

        Assert.True(result);
    }

    [Fact]
    public void SupportsMethod_WithUnsupportedMethod_ShouldReturnFalse()
    {
        var route = new ServiceRoute
        {
            ServiceName = "TestService",
            BaseUrl = "http://localhost:5000",
            Template = "/api/test",
            Methods = new List<string> { "GET", "POST" }
        };

        var result = route.SupportsMethod("DELETE");

        Assert.False(result);
    }

    [Fact]
    public void SupportsMethod_WithCaseInsensitiveMethod_ShouldReturnTrue()
    {
        var route = new ServiceRoute
        {
            ServiceName = "TestService",
            BaseUrl = "http://localhost:5000",
            Template = "/api/test",
            Methods = new List<string> { "GET", "POST" }
        };

        var result = route.SupportsMethod("get");

        Assert.True(result);
    }

    [Theory]
    [InlineData("GET", true)]
    [InlineData("POST", true)]
    [InlineData("PUT", false)]
    [InlineData("DELETE", false)]
    [InlineData("PATCH", false)]
    public void SupportsMethod_WithDifferentMethods_ShouldReturnExpectedResult(string method, bool expected)
    {
        var route = new ServiceRoute
        {
            ServiceName = "TestService",
            BaseUrl = "http://localhost:5000",
            Template = "/api/test",
            Methods = new List<string> { "GET", "POST" }
        };

        var result = route.SupportsMethod(method);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        var route = new ServiceRoute();

        Assert.NotNull(route.Methods);
        Assert.Empty(route.Methods);
        Assert.Equal(30, route.TimeoutSeconds);
    }

    [Fact]
    public void Constructor_WithCustomValues_ShouldSetProperties()
    {
        var methods = new List<string> { "GET", "POST" };
        var route = new ServiceRoute
        {
            ServiceName = "TestService",
            BaseUrl = "http://localhost:5000",
            Template = "/api/test",
            Methods = methods,
            TimeoutSeconds = 60
        };

        Assert.Equal("TestService", route.ServiceName);
        Assert.Equal("http://localhost:5000", route.BaseUrl);
        Assert.Equal("/api/test", route.Template);
        Assert.Equal(methods, route.Methods);
        Assert.Equal(60, route.TimeoutSeconds);
    }
}
