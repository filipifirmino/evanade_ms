using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sales.Application.Settings;
using Sales.Infrastructure.Rabbit.BackgroundServices;

namespace Sales.Tests.InfraStructureTests.RabbitTests;

public class RabbitMqBackgroundServiceTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<RabbitMqBackgroundService>> _loggerMock;
    private readonly Mock<IOptions<RabbitMq>> _settingsMock;
    private readonly RabbitMq _rabbitSettings;

    public RabbitMqBackgroundServiceTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<RabbitMqBackgroundService>>();
        _settingsMock = new Mock<IOptions<RabbitMq>>();

        _rabbitSettings = new RabbitMq
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
            Queues = new List<QueueConfiguration>
            {
                new QueueConfiguration
                {
                    Name = "inventory-stock-update-confirmed",
                    Exchange = "inventory-exchange",
                    RoutingKey = "stock.update.confirmed"
                }
            }
        };

        _settingsMock.Setup(x => x.Value).Returns(_rabbitSettings);
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        var backgroundService = new RabbitMqBackgroundService(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _settingsMock.Object);

        Assert.NotNull(backgroundService);
    }

    [Fact]
    public async Task StopAsync_ShouldLogStopMessage()
    {
        var backgroundService = new RabbitMqBackgroundService(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _settingsMock.Object);

        var cancellationToken = new CancellationToken();

        await backgroundService.StopAsync(cancellationToken);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Parando RabbitMQ Background Service")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_ShouldInitializeWithCorrectSettings()
    {
        var backgroundService = new RabbitMqBackgroundService(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            _settingsMock.Object);

        Assert.NotNull(backgroundService);
        _settingsMock.Verify(x => x.Value, Times.Once);
    }

    [Fact]
    public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new RabbitMqBackgroundService(
            null!,
            _loggerMock.Object,
            _settingsMock.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new RabbitMqBackgroundService(
            _serviceProviderMock.Object,
            null!,
            _settingsMock.Object));
    }

    [Fact]
    public void Constructor_WithNullSettings_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new RabbitMqBackgroundService(
            _serviceProviderMock.Object,
            _loggerMock.Object,
            null!));
    }
}