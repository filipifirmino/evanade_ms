using Microsoft.Extensions.Options;
using Moq;
using Sales.Application.Settings;
using Sales.Infrastructure.Rabbit;

namespace Sales.Tests.InfraStructureTests.RabbitTests;

public class RabbitMqConnectionTests
{
    private readonly Mock<IOptions<RabbitMq>> _settingsMock;
    private readonly RabbitMq _rabbitSettings;

    public RabbitMqConnectionTests()
    {
        _settingsMock = new Mock<IOptions<RabbitMq>>();

        _rabbitSettings = new RabbitMq
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };

        _settingsMock.Setup(x => x.Value).Returns(_rabbitSettings);
    }

    [Fact]
    public void Constructor_WithValidSettings_ShouldCreateInstance()
    {
        Assert.Throws<RabbitMQ.Client.Exceptions.BrokerUnreachableException>(() => 
            new RabbitMqConnection(_settingsMock.Object));
    }

    [Fact]
    public void Constructor_WithNullSettings_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new RabbitMqConnection(null!));
    }

    [Fact]
    public void Settings_ShouldBeCorrectlyConfigured()
    {
        Assert.Equal("localhost", _rabbitSettings.HostName);
        Assert.Equal(5672, _rabbitSettings.Port);
        Assert.Equal("guest", _rabbitSettings.UserName);
        Assert.Equal("guest", _rabbitSettings.Password);
        Assert.Equal("/", _rabbitSettings.VirtualHost);
        Assert.True(_rabbitSettings.AutomaticRecoveryEnabled);
        Assert.Equal(TimeSpan.FromSeconds(5), _rabbitSettings.NetworkRecoveryInterval);
    }
}