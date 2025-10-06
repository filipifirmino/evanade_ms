using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using APIGateway.ApplicationCore.Abstractions;
using APIGateway.Infra.Http;
using AutoBogus;
using Xunit;

namespace APIGateway.Tests.Infra.Tests.Http;

public class HttpProxyServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ILogger<HttpProxyService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IRouteConfigurationService> _routeConfigurationServiceMock;
    private readonly HttpProxyService _httpProxyService;

    public HttpProxyServiceTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _loggerMock = new Mock<ILogger<HttpProxyService>>();
        _configurationMock = new Mock<IConfiguration>();
        _routeConfigurationServiceMock = new Mock<IRouteConfigurationService>();
        _httpProxyService = new HttpProxyService(_httpClientFactoryMock.Object, _loggerMock.Object, _configurationMock.Object, _routeConfigurationServiceMock.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldInitialize()
    {
        Assert.NotNull(_httpProxyService);
    }

}
