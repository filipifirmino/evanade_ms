using System.Text.Json;
using Inventory.Application.Events;
using Inventory.Application.Events.Abstractions;
using Inventory.InfraStructure.Rabbit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Inventory.Tests.InfraStructureTests.RabbitTests;

public class RabbitMqServiceUnitTests : IDisposable
{
    private readonly Mock<IOptions<RabbitMqSettings>> _mockOptions;
    private readonly Mock<ILogger<RabbitMqService>> _mockLogger;
    private readonly RabbitMqSettings _settings;
    private readonly RabbitMqService _rabbitMqService;

    public RabbitMqServiceUnitTests()
    {
        _mockOptions = new Mock<IOptions<RabbitMqSettings>>();
        _mockLogger = new Mock<ILogger<RabbitMqService>>();
        
        _settings = new RabbitMqSettings
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            Port = 5672,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _mockOptions.Setup(x => x.Value).Returns(_settings);
        _rabbitMqService = new RabbitMqService(_mockOptions.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidSettings_ShouldCreateInstance()
    {
        Assert.NotNull(_rabbitMqService);
    }


    [Fact]
    public void Dispose_ShouldNotThrowException()
    {
        var exception = Record.Exception(() => _rabbitMqService.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_MultipleCalls_ShouldNotThrowException()
    {
        _rabbitMqService.Dispose();
        var exception = Record.Exception(() => _rabbitMqService.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    public void JsonSerialization_ShouldUseCorrectOptions()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 100,
            NewStockQuantity = 50,
            Status = "Confirmed"
        };

        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        Assert.NotNull(json);
        Assert.Contains("orderId", json);
        Assert.Contains("productId", json);
        Assert.Contains("productName", json);
        Assert.Contains("quantityReserved", json);
        Assert.Contains("newStockQuantity", json);
        Assert.Contains("status", json);
    }

    [Fact]
    public void JsonDeserialization_ShouldWorkCorrectly()
    {
        var originalMessage = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 150,
            NewStockQuantity = 75,
            Status = "Confirmed"
        };

        var json = JsonSerializer.Serialize(originalMessage, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        var deserializedMessage = JsonSerializer.Deserialize<StockUpdateConfirmedEvent>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        Assert.NotNull(deserializedMessage);
        Assert.Equal(originalMessage.OrderId, deserializedMessage.OrderId);
        Assert.Equal(originalMessage.ProductId, deserializedMessage.ProductId);
        Assert.Equal(originalMessage.ProductName, deserializedMessage.ProductName);
        Assert.Equal(originalMessage.QuantityReserved, deserializedMessage.QuantityReserved);
        Assert.Equal(originalMessage.NewStockQuantity, deserializedMessage.NewStockQuantity);
        Assert.Equal(originalMessage.Status, deserializedMessage.Status);
    }

    [Fact]
    public void Settings_ShouldHaveCorrectDefaults()
    {
        var defaultSettings = new RabbitMqSettings();

        Assert.Equal("localhost", defaultSettings.HostName);
        Assert.Equal("guest", defaultSettings.UserName);
        Assert.Equal("guest", defaultSettings.Password);
        Assert.Equal("/", defaultSettings.VirtualHost);
        Assert.Equal(5672, defaultSettings.Port);
        Assert.True(defaultSettings.AutomaticRecoveryEnabled);
        Assert.Equal(TimeSpan.FromSeconds(10), defaultSettings.NetworkRecoveryInterval);
    }

    [Fact]
    public void Settings_ShouldAllowCustomValues()
    {
        var customSettings = new RabbitMqSettings
        {
            HostName = "custom-host",
            UserName = "custom-user",
            Password = "custom-password",
            VirtualHost = "custom-vhost",
            Port = 1234,
            AutomaticRecoveryEnabled = false,
            NetworkRecoveryInterval = TimeSpan.FromMinutes(5)
        };

        Assert.Equal("custom-host", customSettings.HostName);
        Assert.Equal("custom-user", customSettings.UserName);
        Assert.Equal("custom-password", customSettings.Password);
        Assert.Equal("custom-vhost", customSettings.VirtualHost);
        Assert.Equal(1234, customSettings.Port);
        Assert.False(customSettings.AutomaticRecoveryEnabled);
        Assert.Equal(TimeSpan.FromMinutes(5), customSettings.NetworkRecoveryInterval);
    }

    public void Dispose()
    {
        _rabbitMqService?.Dispose();
    }
}