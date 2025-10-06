using Inventory.Application.Events;
using Inventory.Application.Events.Abstractions;
using Inventory.InfraStructure.Rabbit.Bases;
using Moq;

namespace Inventory.Tests.InfraStructureTests.RabbitTests;

public class TestPublisher : BasePublisher<StockUpdateConfirmedEvent>
{
    public TestPublisher(IRabbitMqService service) : base(service)
    {
    }

    protected override string QueueName => "test-queue";

    public void TestPublish(StockUpdateConfirmedEvent message)
    {
        Publish(message);
    }
}

public class BasePublisherUnitTests
{
    private readonly Mock<IRabbitMqService> _mockRabbitMqService;
    private readonly TestPublisher _publisher;

    public BasePublisherUnitTests()
    {
        _mockRabbitMqService = new Mock<IRabbitMqService>();
        _publisher = new TestPublisher(_mockRabbitMqService.Object);
    }

    [Fact]
    public void Constructor_WithValidService_ShouldCreateInstance()
    {
        Assert.NotNull(_publisher);
    }


    [Fact]
    public void Publish_WithValidMessage_ShouldCallServicePublish()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 100,
            NewStockQuantity = 50
        };

        _publisher.TestPublish(message);

        _mockRabbitMqService.Verify(
            x => x.Publish("test-queue", message),
            Times.Once);
    }


    [Fact]
    public void Publish_WithEmptyOrderId_ShouldCallServicePublish()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.Empty,
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 50,
            NewStockQuantity = 25
        };

        _publisher.TestPublish(message);

        _mockRabbitMqService.Verify(
            x => x.Publish("test-queue", message),
            Times.Once);
    }

    [Fact]
    public void Publish_WithZeroQuantity_ShouldCallServicePublish()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 0,
            NewStockQuantity = 0
        };

        _publisher.TestPublish(message);

        _mockRabbitMqService.Verify(
            x => x.Publish("test-queue", message),
            Times.Once);
    }

    [Fact]
    public void Publish_WithNegativeQuantity_ShouldCallServicePublish()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = -10,
            NewStockQuantity = -5
        };

        _publisher.TestPublish(message);

        _mockRabbitMqService.Verify(
            x => x.Publish("test-queue", message),
            Times.Once);
    }

    [Fact]
    public void Publish_WithLargeQuantity_ShouldCallServicePublish()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = int.MaxValue,
            NewStockQuantity = int.MaxValue
        };

        _publisher.TestPublish(message);

        _mockRabbitMqService.Verify(
            x => x.Publish("test-queue", message),
            Times.Once);
    }

    [Fact]
    public void Publish_WithServiceException_ShouldPropagateException()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 100,
            NewStockQuantity = 50
        };

        _mockRabbitMqService
            .Setup(x => x.Publish(It.IsAny<string>(), It.IsAny<StockUpdateConfirmedEvent>()))
            .Throws(new Exception("Service error"));

        var exception = Assert.Throws<Exception>(() => _publisher.TestPublish(message));
        Assert.Equal("Service error", exception.Message);
    }

    [Fact]
    public void Publish_MultipleMessages_ShouldCallServicePublishForEach()
    {
        var messages = new List<StockUpdateConfirmedEvent>
        {
            new() { OrderId = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 1", QuantityReserved = 10, NewStockQuantity = 5 },
            new() { OrderId = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 2", QuantityReserved = 20, NewStockQuantity = 10 },
            new() { OrderId = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Product 3", QuantityReserved = 30, NewStockQuantity = 15 }
        };

        foreach (var message in messages)
        {
            _publisher.TestPublish(message);
        }

        _mockRabbitMqService.Verify(
            x => x.Publish("test-queue", It.IsAny<StockUpdateConfirmedEvent>()),
            Times.Exactly(3));
    }

    [Fact]
    public void Publish_WithSameMessageMultipleTimes_ShouldCallServicePublishMultipleTimes()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 100,
            NewStockQuantity = 50
        };

        _publisher.TestPublish(message);
        _publisher.TestPublish(message);
        _publisher.TestPublish(message);

        _mockRabbitMqService.Verify(
            x => x.Publish("test-queue", message),
            Times.Exactly(3));
    }

    [Theory]
    [InlineData("Confirmed")]
    [InlineData("Pending")]
    [InlineData("Failed")]
    public void Publish_WithDifferentStatusValues_ShouldCallServicePublish(string status)
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 100,
            NewStockQuantity = 50,
            Status = status
        };

        _publisher.TestPublish(message);

        _mockRabbitMqService.Verify(
            x => x.Publish("test-queue", message),
            Times.Once);
    }

    [Fact]
    public void Publish_WithComplexMessage_ShouldCallServicePublish()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Complex Product Name with Special Characters: ção, ñ, ü",
            QuantityReserved = 999,
            NewStockQuantity = 500,
            Status = "Confirmed",
            ConfirmedAt = DateTime.UtcNow
        };

        _publisher.TestPublish(message);

        _mockRabbitMqService.Verify(
            x => x.Publish("test-queue", message),
            Times.Once);
    }

    [Fact]
    public void Publish_ShouldUseCorrectQueueName()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 100,
            NewStockQuantity = 50
        };

        _publisher.TestPublish(message);

        _mockRabbitMqService.Verify(
            x => x.Publish("test-queue", message),
            Times.Once);
        
        _mockRabbitMqService.Verify(
            x => x.Publish(It.Is<string>(s => s != "test-queue"), It.IsAny<StockUpdateConfirmedEvent>()),
            Times.Never);
    }

    [Fact]
    public void Publish_WithServiceTimeout_ShouldPropagateException()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 100,
            NewStockQuantity = 50
        };

        _mockRabbitMqService
            .Setup(x => x.Publish(It.IsAny<string>(), It.IsAny<StockUpdateConfirmedEvent>()))
            .Throws(new TimeoutException("Service timeout"));

        var exception = Assert.Throws<TimeoutException>(() => _publisher.TestPublish(message));
        Assert.Equal("Service timeout", exception.Message);
    }

    [Fact]
    public void Publish_WithInvalidOperationException_ShouldPropagateException()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 100,
            NewStockQuantity = 50
        };

        _mockRabbitMqService
            .Setup(x => x.Publish(It.IsAny<string>(), It.IsAny<StockUpdateConfirmedEvent>()))
            .Throws(new InvalidOperationException("Invalid operation"));

        var exception = Assert.Throws<InvalidOperationException>(() => _publisher.TestPublish(message));
        Assert.Equal("Invalid operation", exception.Message);
    }

    [Fact]
    public void Publish_WithArgumentException_ShouldPropagateException()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 100,
            NewStockQuantity = 50
        };

        _mockRabbitMqService
            .Setup(x => x.Publish(It.IsAny<string>(), It.IsAny<StockUpdateConfirmedEvent>()))
            .Throws(new ArgumentException("Invalid argument"));

        var exception = Assert.Throws<ArgumentException>(() => _publisher.TestPublish(message));
        Assert.Equal("Invalid argument", exception.Message);
    }

    [Fact]
    public async Task Publish_WithConcurrentCalls_ShouldCallServicePublishForEach()
    {
        var messages = new List<StockUpdateConfirmedEvent>();
        for (int i = 0; i < 10; i++)
        {
            messages.Add(new StockUpdateConfirmedEvent
            {
                OrderId = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductName = $"Product {i}",
                QuantityReserved = i * 10,
                NewStockQuantity = i * 5
            });
        }

        var tasks = messages.Select(message => Task.Run(() => _publisher.TestPublish(message)));
        await Task.WhenAll(tasks);

        _mockRabbitMqService.Verify(
            x => x.Publish("test-queue", It.IsAny<StockUpdateConfirmedEvent>()),
            Times.Exactly(10));
    }

    [Fact]
    public void Publish_ShouldMaintainMessageIntegrity()
    {
        var originalMessage = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 150,
            NewStockQuantity = 75,
            Status = "Confirmed",
            ConfirmedAt = DateTime.UtcNow
        };

        StockUpdateConfirmedEvent? capturedMessage = null;
        _mockRabbitMqService
            .Setup(x => x.Publish(It.IsAny<string>(), It.IsAny<StockUpdateConfirmedEvent>()))
            .Callback<string, StockUpdateConfirmedEvent>((queue, message) => capturedMessage = message);

        _publisher.TestPublish(originalMessage);

        Assert.NotNull(capturedMessage);
        Assert.Equal(originalMessage.OrderId, capturedMessage.OrderId);
        Assert.Equal(originalMessage.ProductId, capturedMessage.ProductId);
        Assert.Equal(originalMessage.ProductName, capturedMessage.ProductName);
        Assert.Equal(originalMessage.QuantityReserved, capturedMessage.QuantityReserved);
        Assert.Equal(originalMessage.NewStockQuantity, capturedMessage.NewStockQuantity);
        Assert.Equal(originalMessage.Status, capturedMessage.Status);
    }

    [Fact]
    public void Publish_WithDifferentQueueNames_ShouldUseCorrectQueue()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 100,
            NewStockQuantity = 50
        };

        _publisher.TestPublish(message);

        _mockRabbitMqService.Verify(
            x => x.Publish("test-queue", message),
            Times.Once);
        
        _mockRabbitMqService.Verify(
            x => x.Publish(It.Is<string>(s => s != "test-queue"), It.IsAny<StockUpdateConfirmedEvent>()),
            Times.Never);
    }

    [Fact]
    public void Publish_WithNullService_ShouldThrowException()
    {
        var publisher = new TestPublisher(null!);
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 100,
            NewStockQuantity = 50
        };

        Assert.Throws<NullReferenceException>(() => publisher.TestPublish(message));
    }

    [Fact]
    public void Publish_WithServiceReturningVoid_ShouldCompleteSuccessfully()
    {
        var message = new StockUpdateConfirmedEvent
        {
            OrderId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Test Product",
            QuantityReserved = 100,
            NewStockQuantity = 50
        };

        _mockRabbitMqService
            .Setup(x => x.Publish(It.IsAny<string>(), It.IsAny<StockUpdateConfirmedEvent>()));

        var exception = Record.Exception(() => _publisher.TestPublish(message));
        Assert.Null(exception);
    }

}