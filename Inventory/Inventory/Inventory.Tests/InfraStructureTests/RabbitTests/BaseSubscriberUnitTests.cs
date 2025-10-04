using Inventory.Application.Events;
using Inventory.Application.Events.Abstractions;
using Inventory.InfraStructure.Rabbit.Bases;
using Moq;

namespace Inventory.Tests.InfraStructureTests.RabbitTests;

public class TestSubscriber : BaseSubscriber<OrderCreatedEvent>
{
    public TestSubscriber(IRabbitMqService service) : base(service)
    {
    }

    protected override string QueueName => "test-queue";

    public Action<OrderCreatedEvent>? LastHandledMessage { get; private set; }

    protected override void HandleMessage(OrderCreatedEvent message)
    {
        LastHandledMessage?.Invoke(message);
    }

    public void TestStartListening()
    {
        StartListening();
    }

    public void SetMessageHandler(Action<OrderCreatedEvent> handler)
    {
        LastHandledMessage = handler;
    }
}

public class BaseSubscriberUnitTests
{
    private readonly Mock<IRabbitMqService> _mockRabbitMqService;
    private readonly TestSubscriber _subscriber;

    public BaseSubscriberUnitTests()
    {
        _mockRabbitMqService = new Mock<IRabbitMqService>();
        _subscriber = new TestSubscriber(_mockRabbitMqService.Object);
    }

    [Fact]
    public void Constructor_WithValidService_ShouldCreateInstance()
    {
        Assert.NotNull(_subscriber);
    }


    [Fact]
    public void StartListening_ShouldCallServiceSubscribe()
    {
        _subscriber.TestStartListening();

        _mockRabbitMqService.Verify(
            x => x.Subscribe("test-queue", It.IsAny<Action<OrderCreatedEvent>>()),
            Times.Once);
    }

    [Fact]
    public void HandleMessage_WithValidMessage_ShouldInvokeHandler()
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

        var handlerInvoked = false;
        _subscriber.SetMessageHandler(msg => handlerInvoked = true);

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        Assert.True(handlerInvoked);
    }


    [Fact]
    public void HandleMessage_WithEmptyOrderId_ShouldInvokeHandler()
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

        var handlerInvoked = false;
        _subscriber.SetMessageHandler(msg => handlerInvoked = true);

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        Assert.True(handlerInvoked);
    }

    [Fact]
    public void HandleMessage_WithZeroTotalAmount_ShouldInvokeHandler()
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

        var handlerInvoked = false;
        _subscriber.SetMessageHandler(msg => handlerInvoked = true);

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        Assert.True(handlerInvoked);
    }

    [Fact]
    public void HandleMessage_WithNegativeTotalAmount_ShouldInvokeHandler()
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

        var handlerInvoked = false;
        _subscriber.SetMessageHandler(msg => handlerInvoked = true);

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        Assert.True(handlerInvoked);
    }

    [Fact]
    public void HandleMessage_WithLargeTotalAmount_ShouldInvokeHandler()
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

        var handlerInvoked = false;
        _subscriber.SetMessageHandler(msg => handlerInvoked = true);

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        Assert.True(handlerInvoked);
    }

    [Fact]
    public void HandleMessage_WithHandlerException_ShouldPropagateException()
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

        var expectedException = new Exception("Handler error");
        _subscriber.SetMessageHandler(msg => throw expectedException);

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        var exception = Assert.Throws<Exception>(() => capturedCallback(message));
        Assert.Equal("Handler error", exception.Message);
    }

    [Fact]
    public void HandleMessage_WithHandlerArgumentException_ShouldPropagateException()
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

        var expectedException = new ArgumentException("Invalid argument");
        _subscriber.SetMessageHandler(msg => throw expectedException);

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        var exception = Assert.Throws<ArgumentException>(() => capturedCallback(message));
        Assert.Equal("Invalid argument", exception.Message);
    }

    [Fact]
    public void HandleMessage_WithHandlerInvalidOperationException_ShouldPropagateException()
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

        var expectedException = new InvalidOperationException("Invalid operation");
        _subscriber.SetMessageHandler(msg => throw expectedException);

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        var exception = Assert.Throws<InvalidOperationException>(() => capturedCallback(message));
        Assert.Equal("Invalid operation", exception.Message);
    }

    [Fact]
    public void HandleMessage_MultipleMessages_ShouldInvokeHandlerForEach()
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

        var handlerInvokeCount = 0;
        _subscriber.SetMessageHandler(msg => handlerInvokeCount++);

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        foreach (var message in messages)
        {
            capturedCallback(message);
        }

        Assert.Equal(3, handlerInvokeCount);
    }

    [Fact]
    public void HandleMessage_WithSameMessageMultipleTimes_ShouldInvokeHandlerMultipleTimes()
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

        var handlerInvokeCount = 0;
        _subscriber.SetMessageHandler(msg => handlerInvokeCount++);

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);
        capturedCallback(message);
        capturedCallback(message);

        Assert.Equal(3, handlerInvokeCount);
    }

    [Theory]
    [InlineData("Product A")]
    [InlineData("Product B")]
    [InlineData("Complex Product Name")]
    public void HandleMessage_WithDifferentProductNames_ShouldInvokeHandler(string productName)
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

        var handlerInvoked = false;
        _subscriber.SetMessageHandler(msg => handlerInvoked = true);

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        Assert.True(handlerInvoked);
    }

    [Fact]
    public void HandleMessage_WithComplexMessage_ShouldInvokeHandler()
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

        var handlerInvoked = false;
        _subscriber.SetMessageHandler(msg => handlerInvoked = true);

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(message);

        Assert.True(handlerInvoked);
    }

    [Fact]
    public void HandleMessage_ShouldUseCorrectQueueName()
    {
        _subscriber.TestStartListening();

        _mockRabbitMqService.Verify(
            x => x.Subscribe("test-queue", It.IsAny<Action<OrderCreatedEvent>>()),
            Times.Once);
        
        _mockRabbitMqService.Verify(
            x => x.Subscribe(It.Is<string>(s => s != "test-queue"), It.IsAny<Action<OrderCreatedEvent>>()),
            Times.Never);
    }

    [Fact]
    public void HandleMessage_WithServiceException_ShouldPropagateException()
    {
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Throws(new Exception("Service error"));

        var exception = Assert.Throws<Exception>(() => _subscriber.TestStartListening());
        Assert.Equal("Service error", exception.Message);
    }

    [Fact]
    public void HandleMessage_WithServiceTimeoutException_ShouldPropagateException()
    {
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Throws(new TimeoutException("Service timeout"));

        var exception = Assert.Throws<TimeoutException>(() => _subscriber.TestStartListening());
        Assert.Equal("Service timeout", exception.Message);
    }

    [Fact]
    public void HandleMessage_WithServiceInvalidOperationException_ShouldPropagateException()
    {
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Throws(new InvalidOperationException("Invalid operation"));

        var exception = Assert.Throws<InvalidOperationException>(() => _subscriber.TestStartListening());
        Assert.Equal("Invalid operation", exception.Message);
    }

    [Fact]
    public void HandleMessage_WithServiceArgumentException_ShouldPropagateException()
    {
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Throws(new ArgumentException("Invalid argument"));

        var exception = Assert.Throws<ArgumentException>(() => _subscriber.TestStartListening());
        Assert.Equal("Invalid argument", exception.Message);
    }

    [Fact]
    public async Task HandleMessage_WithConcurrentMessages_ShouldInvokeHandlerForEach()
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

        var handlerInvokeCount = 0;
        _subscriber.SetMessageHandler(msg => Interlocked.Increment(ref handlerInvokeCount));

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        var tasks = messages.Select(message => Task.Run(() => capturedCallback(message)));
        await Task.WhenAll(tasks);

        Assert.Equal(10, handlerInvokeCount);
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

        OrderCreatedEvent? capturedMessage = null;
        _subscriber.SetMessageHandler(msg => capturedMessage = msg);

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        capturedCallback(originalMessage);

        Assert.NotNull(capturedMessage);
        Assert.Equal(originalMessage.OrderId, capturedMessage.OrderId);
        Assert.Equal(originalMessage.CustomerId, capturedMessage.CustomerId);
        Assert.Equal(originalMessage.TotalAmount, capturedMessage.TotalAmount);
        Assert.Equal(originalMessage.Items.Count, capturedMessage.Items.Count);
    }

    [Fact]
    public void HandleMessage_WithNullHandler_ShouldNotThrowException()
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

        _subscriber.SetMessageHandler(null!);

        Action<OrderCreatedEvent>? capturedCallback = null;
        _mockRabbitMqService
            .Setup(x => x.Subscribe(It.IsAny<string>(), It.IsAny<Action<OrderCreatedEvent>>()))
            .Callback<string, Action<OrderCreatedEvent>>((queue, callback) => capturedCallback = callback);

        _subscriber.TestStartListening();

        Assert.NotNull(capturedCallback);
        var exception = Record.Exception(() => capturedCallback(message));
        Assert.Null(exception);
    }

}