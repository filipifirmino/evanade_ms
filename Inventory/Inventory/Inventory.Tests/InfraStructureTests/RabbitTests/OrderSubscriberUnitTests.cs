using Inventory.Application.Events;
using Inventory.Application.Events.Abstractions;
using Inventory.Application.UseCases.Abstractions;
using Inventory.InfraStructure.Rabbit.Subscriber;
using Microsoft.Extensions.Logging;
using Moq;

namespace Inventory.Tests.InfraStructureTests.RabbitTests;

public class OrderSubscriberUnitTests
{
    private readonly Mock<IRabbitMqService> _mockRabbitMqService;
    private readonly Mock<IProcessOrderCreated> _mockProcessOrderCreated;
    private readonly Mock<ILogger<OrderSubscriber>> _mockLogger;
    private readonly OrderSubscriber _subscriber;

    public OrderSubscriberUnitTests()
    {
        _mockRabbitMqService = new Mock<IRabbitMqService>();
        _mockProcessOrderCreated = new Mock<IProcessOrderCreated>();
        _mockLogger = new Mock<ILogger<OrderSubscriber>>();
        _subscriber = new OrderSubscriber(_mockRabbitMqService.Object, _mockLogger.Object, _mockProcessOrderCreated.Object);
    }

    [Fact]
    public void Constructor_WithValidServices_ShouldCreateInstance()
    {
        Assert.NotNull(_subscriber);
    }


    [Fact]
    public void StartListening_ShouldCallServiceSubscribe()
    {
        _subscriber.StartListening();

        _mockRabbitMqService.Verify(
            x => x.Subscribe("order-created-queue", It.IsAny<Action<OrderCreatedEvent>>()),
            Times.Once);
    }

    [Fact]
    public void HandleMessage_WithValidMessage_ShouldCallProcessOrderCreated()
    {
        var message = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 100.00m,
            Items = new List<OrderItemEvent>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = 100,
                    UnitPrice = 1.00m
                }
            }
        };

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.StartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        _mockProcessOrderCreated.Verify(
            x => x.ExecuteAsync(message),
            Times.Once);
    }


    [Fact]
    public void HandleMessage_WithEmptyOrderId_ShouldCallProcessOrderCreated()
    {
        var message = new OrderCreatedEvent
        {
            OrderId = Guid.Empty,
            CustomerId = Guid.NewGuid(),
            TotalAmount = 50.00m,
            Items = new List<OrderItemEvent>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = 50,
                    UnitPrice = 1.00m
                }
            }
        };

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.StartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        _mockProcessOrderCreated.Verify(
            x => x.ExecuteAsync(message),
            Times.Once);
    }

    [Fact]
    public void HandleMessage_WithZeroTotalAmount_ShouldCallProcessOrderCreated()
    {
        var message = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 0,
            Items = new List<OrderItemEvent>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = 0,
                    UnitPrice = 0
                }
            }
        };

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.StartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        _mockProcessOrderCreated.Verify(
            x => x.ExecuteAsync(message),
            Times.Once);
    }

    [Fact]
    public void HandleMessage_WithNegativeTotalAmount_ShouldCallProcessOrderCreated()
    {
        var message = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = -10.00m,
            Items = new List<OrderItemEvent>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = -10,
                    UnitPrice = -1.00m
                }
            }
        };

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.StartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        _mockProcessOrderCreated.Verify(
            x => x.ExecuteAsync(message),
            Times.Once);
    }

    [Fact]
    public void HandleMessage_WithLargeTotalAmount_ShouldCallProcessOrderCreated()
    {
        var message = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = decimal.MaxValue,
            Items = new List<OrderItemEvent>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = int.MaxValue,
                    UnitPrice = decimal.MaxValue
                }
            }
        };

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.StartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        _mockProcessOrderCreated.Verify(
            x => x.ExecuteAsync(message),
            Times.Once);
    }




    [Fact]
    public void HandleMessage_MultipleMessages_ShouldCallProcessOrderCreatedForEach()
    {
        var messages = new List<OrderCreatedEvent>
        {
            new() 
            { 
                OrderId = Guid.NewGuid(), 
                CustomerId = Guid.NewGuid(), 
                TotalAmount = 10.00m,
                Items = new List<OrderItemEvent>
                {
                    new() { ProductId = Guid.NewGuid(), ProductName = "Product 1", Quantity = 10, UnitPrice = 1.00m }
                }
            },
            new() 
            { 
                OrderId = Guid.NewGuid(), 
                CustomerId = Guid.NewGuid(), 
                TotalAmount = 20.00m,
                Items = new List<OrderItemEvent>
                {
                    new() { ProductId = Guid.NewGuid(), ProductName = "Product 2", Quantity = 20, UnitPrice = 1.00m }
                }
            },
            new() 
            { 
                OrderId = Guid.NewGuid(), 
                CustomerId = Guid.NewGuid(), 
                TotalAmount = 30.00m,
                Items = new List<OrderItemEvent>
                {
                    new() { ProductId = Guid.NewGuid(), ProductName = "Product 3", Quantity = 30, UnitPrice = 1.00m }
                }
            }
        };

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.StartListening();

        Assert.NotNull(capturedCallback);
        foreach (var message in messages)
        {
            capturedCallback(message);
        }

        _mockProcessOrderCreated.Verify(
            x => x.ExecuteAsync(It.IsAny<OrderCreatedEvent>()),
            Times.Exactly(3));
    }

    [Fact]
    public void HandleMessage_WithSameMessageMultipleTimes_ShouldCallProcessOrderCreatedMultipleTimes()
    {
        var message = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 100.00m,
            Items = new List<OrderItemEvent>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = 100,
                    UnitPrice = 1.00m
                }
            }
        };

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.StartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);
        capturedCallback(message);
        capturedCallback(message);

        _mockProcessOrderCreated.Verify(
            x => x.ExecuteAsync(message),
            Times.Exactly(3));
    }

    [Theory]
    [InlineData("Product A")]
    [InlineData("Product B")]
    [InlineData("Complex Product Name")]
    public void HandleMessage_WithDifferentProductNames_ShouldCallProcessOrderCreated(string productName)
    {
        var message = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 100.00m,
            Items = new List<OrderItemEvent>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = productName,
                    Quantity = 100,
                    UnitPrice = 1.00m
                }
            }
        };

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.StartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        _mockProcessOrderCreated.Verify(
            x => x.ExecuteAsync(message),
            Times.Once);
    }

    [Fact]
    public void HandleMessage_WithComplexMessage_ShouldCallProcessOrderCreated()
    {
        var message = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 999.99m,
            Items = new List<OrderItemEvent>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Complex Product Name with Special Characters: ção, ñ, ü",
                    Quantity = 999,
                    UnitPrice = 1.00m
                }
            }
        };

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.StartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        _mockProcessOrderCreated.Verify(
            x => x.ExecuteAsync(message),
            Times.Once);
    }

    [Fact]
    public void HandleMessage_ShouldUseCorrectQueueName()
    {
        _subscriber.StartListening();

        _mockRabbitMqService.Verify(
            x => x.Subscribe("order-created-queue", It.IsAny<Action<OrderCreatedEvent>>()),
            Times.Once);
        
        _mockRabbitMqService.Verify(
            x => x.Subscribe(It.Is<string>(s => s != "order-created-queue"), It.IsAny<Action<OrderCreatedEvent>>()),
            Times.Never);
    }

    [Fact]
    public void HandleMessage_WithServiceException_ShouldPropagateException()
    {
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Throws(new Exception("Service error"));

        var exception = Assert.Throws<Exception>(() => _subscriber.StartListening());
        Assert.Equal("Service error", exception.Message);
    }

    [Fact]
    public void HandleMessage_WithServiceTimeoutException_ShouldPropagateException()
    {
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Throws(new TimeoutException("Service timeout"));

        var exception = Assert.Throws<TimeoutException>(() => _subscriber.StartListening());
        Assert.Equal("Service timeout", exception.Message);
    }

    [Fact]
    public void HandleMessage_WithServiceInvalidOperationException_ShouldPropagateException()
    {
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Throws(new InvalidOperationException("Invalid operation"));

        var exception = Assert.Throws<InvalidOperationException>(() => _subscriber.StartListening());
        Assert.Equal("Invalid operation", exception.Message);
    }

    [Fact]
    public void HandleMessage_WithServiceArgumentException_ShouldPropagateException()
    {
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Throws(new ArgumentException("Invalid argument"));

        var exception = Assert.Throws<ArgumentException>(() => _subscriber.StartListening());
        Assert.Equal("Invalid argument", exception.Message);
    }

    [Fact]
    public async Task HandleMessage_WithConcurrentMessages_ShouldCallProcessOrderCreatedForEach()
    {
        var messages = new List<OrderCreatedEvent>();
        for (int i = 0; i < 10; i++)
        {
            messages.Add(new OrderCreatedEvent
            {
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                TotalAmount = i * 10.00m,
                Items = new List<OrderItemEvent>
                {
                    new()
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = $"Product {i}",
                        Quantity = i * 10,
                        UnitPrice = 1.00m
                    }
                }
            });
        }

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.StartListening();

        Assert.NotNull(capturedCallback);
        var tasks = messages.Select(message => Task.Run(() => capturedCallback(message)));
        await Task.WhenAll(tasks);

        _mockProcessOrderCreated.Verify(
            x => x.ExecuteAsync(It.IsAny<OrderCreatedEvent>()),
            Times.Exactly(10));
    }

    [Fact]
    public void HandleMessage_ShouldMaintainMessageIntegrity()
    {
        var originalMessage = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 150.00m,
            Items = new List<OrderItemEvent>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = 150,
                    UnitPrice = 1.00m
                }
            }
        };

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.StartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(originalMessage);

        _mockProcessOrderCreated.Verify(
            x => x.ExecuteAsync(It.Is<OrderCreatedEvent>(m => 
                m.OrderId == originalMessage.OrderId &&
                m.CustomerId == originalMessage.CustomerId &&
                m.TotalAmount == originalMessage.TotalAmount &&
                m.Items.Count == originalMessage.Items.Count)),
            Times.Once);
    }

    [Fact]
    public void HandleMessage_ShouldLogInformationOnSuccessfulProcessing()
    {
        var message = new OrderCreatedEvent
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 100.00m,
            Items = new List<OrderItemEvent>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Test Product",
                    Quantity = 100,
                    UnitPrice = 1.00m
                }
            }
        };

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.StartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Mensagem recebida na fila 'order-created-queue'")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}