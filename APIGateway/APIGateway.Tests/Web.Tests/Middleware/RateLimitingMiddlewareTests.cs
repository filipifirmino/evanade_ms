using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using APIGateway.ApplicationCore.Abstractions;
using APIGateway.Web.Middleware;
using Xunit;

namespace APIGateway.Tests.Web.Tests.Middleware;

public class RateLimitingMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<IRateLimitService> _rateLimitServiceMock;
    private readonly Mock<ILogger<RateLimitingMiddleware>> _loggerMock;
    private readonly RateLimitingMiddleware _middleware;

    public RateLimitingMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _rateLimitServiceMock = new Mock<IRateLimitService>();
        _loggerMock = new Mock<ILogger<RateLimitingMiddleware>>();
        _middleware = new RateLimitingMiddleware(_nextMock.Object, _rateLimitServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WithAllowedRequest_ShouldCallNext()
    {
        var context = CreateHttpContext("/api/test");
        _rateLimitServiceMock.Setup(x => x.AllowRequestAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        await _middleware.InvokeAsync(context);

        _nextMock.Verify(x => x(context), Times.Once);
        _rateLimitServiceMock.Verify(x => x.AllowRequestAsync("general", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithAuthPath_ShouldUseAuthCategory()
    {
        var context = CreateHttpContext("/api/auth/login");
        _rateLimitServiceMock.Setup(x => x.AllowRequestAsync("auth", It.IsAny<string>()))
            .ReturnsAsync(true);

        await _middleware.InvokeAsync(context);

        _nextMock.Verify(x => x(context), Times.Once);
        _rateLimitServiceMock.Verify(x => x.AllowRequestAsync("auth", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithInventoryPath_ShouldUseInventoryCategory()
    {
        var context = CreateHttpContext("/api/inventory/products");
        _rateLimitServiceMock.Setup(x => x.AllowRequestAsync("inventory", It.IsAny<string>()))
            .ReturnsAsync(true);

        await _middleware.InvokeAsync(context);

        _nextMock.Verify(x => x(context), Times.Once);
        _rateLimitServiceMock.Verify(x => x.AllowRequestAsync("inventory", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithSalesPath_ShouldUseSalesCategory()
    {
        var context = CreateHttpContext("/api/sales/orders");
        _rateLimitServiceMock.Setup(x => x.AllowRequestAsync("sales", It.IsAny<string>()))
            .ReturnsAsync(true);

        await _middleware.InvokeAsync(context);

        _nextMock.Verify(x => x(context), Times.Once);
        _rateLimitServiceMock.Verify(x => x.AllowRequestAsync("sales", It.IsAny<string>()), Times.Once);
    }


    [Fact]
    public async Task InvokeAsync_WithNullPath_ShouldUseGeneralCategory()
    {
        var context = CreateHttpContext(null);
        _rateLimitServiceMock.Setup(x => x.AllowRequestAsync("general", It.IsAny<string>()))
            .ReturnsAsync(true);

        await _middleware.InvokeAsync(context);

        _nextMock.Verify(x => x(context), Times.Once);
        _rateLimitServiceMock.Verify(x => x.AllowRequestAsync("general", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithUnknownIpAddress_ShouldUseUnknownAsClientId()
    {
        var context = CreateHttpContext("/api/test", null);
        _rateLimitServiceMock.Setup(x => x.AllowRequestAsync(It.IsAny<string>(), "unknown"))
            .ReturnsAsync(true);

        await _middleware.InvokeAsync(context);

        _nextMock.Verify(x => x(context), Times.Once);
        _rateLimitServiceMock.Verify(x => x.AllowRequestAsync("general", "unknown"), Times.Once);
    }

    [Theory]
    [InlineData("/api/auth/login", "auth")]
    [InlineData("/api/auth/register", "auth")]
    [InlineData("/api/inventory/products", "inventory")]
    [InlineData("/api/inventory/stock", "inventory")]
    [InlineData("/api/sales/orders", "sales")]
    [InlineData("/api/sales/reports", "sales")]
    [InlineData("/api/other/endpoint", "general")]
    [InlineData("/api", "general")]
    public async Task InvokeAsync_WithDifferentPaths_ShouldUseCorrectCategory(string path, string expectedCategory)
    {
        var context = CreateHttpContext(path);
        _rateLimitServiceMock.Setup(x => x.AllowRequestAsync(expectedCategory, It.IsAny<string>()))
            .ReturnsAsync(true);

        await _middleware.InvokeAsync(context);

        _rateLimitServiceMock.Verify(x => x.AllowRequestAsync(expectedCategory, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithCaseInsensitivePath_ShouldWork()
    {
        var context = CreateHttpContext("/API/AUTH/LOGIN");
        _rateLimitServiceMock.Setup(x => x.AllowRequestAsync("auth", It.IsAny<string>()))
            .ReturnsAsync(true);

        await _middleware.InvokeAsync(context);

        _rateLimitServiceMock.Verify(x => x.AllowRequestAsync("auth", It.IsAny<string>()), Times.Once);
    }

    private HttpContext CreateHttpContext(string path, string ipAddress = "127.0.0.1")
    {
        var context = new Mock<HttpContext>();
        var request = new Mock<HttpRequest>();
        var connection = new Mock<ConnectionInfo>();
        var response = new Mock<HttpResponse>();
        var headers = new HeaderDictionary();

        request.Setup(x => x.Path).Returns(new PathString(path ?? ""));
        connection.Setup(x => x.RemoteIpAddress).Returns(ipAddress != null ? System.Net.IPAddress.Parse(ipAddress) : null);
        response.SetupProperty(x => x.StatusCode);
        response.Setup(x => x.Headers).Returns(headers);

        context.Setup(x => x.Request).Returns(request.Object);
        context.Setup(x => x.Connection).Returns(connection.Object);
        context.Setup(x => x.Response).Returns(response.Object);

        return context.Object;
    }
}
