using AutoBogus;
using Microsoft.Extensions.Logging;
using Moq;
using Sales.Application.AbstractionRabbit;
using Sales.Application.Events;
using Sales.Infrastructure.Rabbit.Producers;

namespace Sales.Tests.InfraStructureTests.RabbitTests;

public class GenericEventProducerTests
{
    private readonly Mock<IMessageProducer> _messageProducerMock;
    private readonly Mock<ILogger<GenericEventProducer>> _loggerMock;
    private readonly GenericEventProducer _producer;

    public GenericEventProducerTests()
    {
        _messageProducerMock = new Mock<IMessageProducer>();
        _loggerMock = new Mock<ILogger<GenericEventProducer>>();
        
        _producer = new GenericEventProducer(_messageProducerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task PublishEventAsync_WithValidEventWithQueueConfiguration_ShouldPublishSuccessfully()
    {
        var orderCreated = AutoFaker.Generate<OrderCreated>();
        orderCreated.OrderId = Guid.NewGuid();
        orderCreated.CustomerId = Guid.NewGuid().ToString();
        orderCreated.TotalAmount = 100.50m;

        _messageProducerMock.Setup(x => x.PublishAsync(
                It.IsAny<OrderCreated>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _producer.PublishEventAsync(orderCreated);

        _messageProducerMock.Verify(x => x.PublishAsync(
            orderCreated,
            "order-exchange",
            "order.created",
            orderCreated.QueueName,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishEventAsync_WithEventWithoutQueueConfiguration_ShouldThrowInvalidOperationException()
    {
        var invalidEvent = new { OrderId = Guid.NewGuid(), Name = "Test" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _producer.PublishEventAsync(invalidEvent));

        _messageProducerMock.Verify(x => x.PublishAsync(
            It.IsAny<object>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PublishEventAsync_WithCancellationToken_ShouldPassTokenToMessageProducer()
    {
        var orderCreated = AutoFaker.Generate<OrderCreated>();
        var cancellationToken = new CancellationToken();

        _messageProducerMock.Setup(x => x.PublishAsync(
                It.IsAny<OrderCreated>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _producer.PublishEventAsync(orderCreated, cancellationToken);

        _messageProducerMock.Verify(x => x.PublishAsync(
            orderCreated,
            "order-exchange",
            "order.created",
            orderCreated.QueueName,
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task PublishEventAsync_WhenMessageProducerThrowsException_ShouldLogErrorAndRethrow()
    {
        var orderCreated = AutoFaker.Generate<OrderCreated>();
        var exception = new Exception("RabbitMQ connection failed");

        _messageProducerMock.Setup(x => x.PublishAsync(
                It.IsAny<OrderCreated>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        await Assert.ThrowsAsync<Exception>(() => _producer.PublishEventAsync(orderCreated));

        _messageProducerMock.Verify(x => x.PublishAsync(
            orderCreated,
            "order-exchange",
            "order.created",
            orderCreated.QueueName,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishEventAsync_WithOrderConfirmedEvent_ShouldPublishSuccessfully()
    {
        var orderConfirmed = AutoFaker.Generate<OrderConfirmed>();
        orderConfirmed.OrderId = Guid.NewGuid();
        orderConfirmed.Status = Sales.Application.Enums.Status.Confirmed;

        _messageProducerMock.Setup(x => x.PublishAsync(
                It.IsAny<OrderConfirmed>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _producer.PublishEventAsync(orderConfirmed);

        _messageProducerMock.Verify(x => x.PublishAsync(
            orderConfirmed,
            "order-exchange",
            "order.created",
            orderConfirmed.QueueName,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
