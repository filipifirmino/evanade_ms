using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using APIGateway.ApplicationCore.Abstractions;
using APIGateway.Web.Controllers;
using AutoBogus;
using Xunit;

namespace APIGateway.Tests.Web.Tests.Controllers;

public class GatewayControllerTests
{
    private readonly Mock<IProxyService> _proxyServiceMock;
    private readonly Mock<ILogger<GatewayController>> _loggerMock;
    private readonly GatewayController _controller;

    public GatewayControllerTests()
    {
        _proxyServiceMock = new Mock<IProxyService>();
        _loggerMock = new Mock<ILogger<GatewayController>>();
        _controller = new GatewayController(_proxyServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldInitialize()
    {
        Assert.NotNull(_controller);
    }

}
