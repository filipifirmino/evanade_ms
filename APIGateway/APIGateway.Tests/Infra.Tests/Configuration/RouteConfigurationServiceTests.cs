using Microsoft.Extensions.Configuration;
using APIGateway.ApplicationCore.Domain.Entities;
using APIGateway.Infra.Configuration;
using Xunit;

namespace APIGateway.Tests.Infra.Tests.Configuration;

public class RouteConfigurationServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly RouteConfigurationService _routeConfigurationService;

    public RouteConfigurationServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        
        // Setup default configuration to avoid null reference exceptions
        var servicesSection = new Mock<IConfigurationSection>();
        servicesSection.Setup(x => x.GetChildren()).Returns(new List<IConfigurationSection>());
        _configurationMock.Setup(x => x.GetSection("Services")).Returns(servicesSection.Object);
        
        _routeConfigurationService = new RouteConfigurationService(_configurationMock.Object);
    }


    [Fact]
    public void IsServiceConfigured_WithNonExistentService_ShouldReturnFalse()
    {
        var serviceName = "NonExistentService";
        var serviceSection = new Mock<IConfigurationSection>();
        serviceSection.Setup(x => x.GetChildren()).Returns(new List<IConfigurationSection>());

        _configurationMock.Setup(x => x.GetSection($"Services:{serviceName}"))
            .Returns(serviceSection.Object);

        var result = _routeConfigurationService.IsServiceConfigured(serviceName);

        Assert.False(result);
    }

    [Fact]
    public void IsServiceConfigured_WithEmptyBaseUrl_ShouldReturnFalse()
    {
        var serviceName = "TestService";
        var serviceSection = new Mock<IConfigurationSection>();
        serviceSection.Setup(x => x.GetChildren()).Returns(new List<IConfigurationSection>());
        serviceSection.Setup(x => x["BaseUrl"]).Returns("");

        _configurationMock.Setup(x => x.GetSection($"Services:{serviceName}"))
            .Returns(serviceSection.Object);

        var result = _routeConfigurationService.IsServiceConfigured(serviceName);

        Assert.False(result);
    }



    [Fact]
    public void IsRouteAllowed_WithNonExistentService_ShouldReturnFalse()
    {
        var serviceName = "NonExistentService";
        var path = "/api/test";
        var method = HttpMethod.Get;

        var result = _routeConfigurationService.IsRouteAllowed(serviceName, path, method);

        Assert.False(result);
    }

    [Fact]
    public void GetServiceBaseUrl_WithExistingService_ShouldReturnBaseUrl()
    {
        var serviceName = "TestService";
        var expectedBaseUrl = "http://localhost:5000";

        _configurationMock.Setup(x => x[$"Services:{serviceName}:BaseUrl"])
            .Returns(expectedBaseUrl);

        var result = _routeConfigurationService.GetServiceBaseUrl(serviceName);

        Assert.Equal(expectedBaseUrl, result);
    }
    

    [Fact]
    public void GetDownstreamBasePath_WithExistingPath_ShouldReturnPath()
    {
        var serviceName = "TestService";
        var expectedPath = "/api/v1";

        _configurationMock.Setup(x => x[$"Services:{serviceName}:DownstreamBasePath"])
            .Returns(expectedPath);

        var result = _routeConfigurationService.GetDownstreamBasePath(serviceName);

        Assert.Equal(expectedPath, result);
    }

    [Fact]
    public void GetDownstreamBasePath_WithNonExistentPath_ShouldReturnEmptyString()
    {
        var serviceName = "TestService";

        _configurationMock.Setup(x => x[$"Services:{serviceName}:DownstreamBasePath"])
            .Returns((string)null);

        var result = _routeConfigurationService.GetDownstreamBasePath(serviceName);

        Assert.Equal(string.Empty, result);
    }


}
