using AutoBogus;
using Microsoft.Extensions.Logging;
using Moq;
using Sales.Application.Events;
using Sales.Application.UseCases.Abstractions;
using Sales.Infrastructure.Rabbit.Consumers;

namespace Sales.Tests.InfraStructureTests.RabbitTests;

public class OrderConfirmedSubscriberTests
{
    private readonly Mock<ILogger<OrderConfirmedSubscriber>> _loggerMock;
    private readonly Mock<IOrderConfirmedProcess> _orderConfirmedProcessMock;
    private readonly OrderConfirmedSubscriber _subscriber;

    public OrderConfirmedSubscriberTests()
    {
        _loggerMock = new Mock<ILogger<OrderConfirmedSubscriber>>();
        _orderConfirmedProcessMock = new Mock<IOrderConfirmedProcess>();
        
        _subscriber = new OrderConfirmedSubscriber(
            _loggerMock.Object,
            _orderConfirmedProcessMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidMessage_ShouldProcessOrderSuccessfully()
    {
        var message = AutoFaker.Generate<OrderConfirmed>();
        message.OrderId = Guid.NewGuid();
        message.Status = Sales.Application.Enums.Status.Confirmed;

        _orderConfirmedProcessMock.Setup(x => x.HandleOrder(message.OrderId, message.Status))
            .ReturnsAsync(new Sales.Application.ValueObject.Result<Sales.Application.Entities.Order>(true, "Success", AutoFaker.Generate<Sales.Application.Entities.Order>()));

        await _subscriber.HandleAsync(message);

        _orderConfirmedProcessMock.Verify(x => x.HandleOrder(message.OrderId, message.Status), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenOrderProcessThrowsException_ShouldLogErrorAndRethrow()
    {
        var message = AutoFaker.Generate<OrderConfirmed>();
        message.OrderId = Guid.NewGuid();
        message.Status = Sales.Application.Enums.Status.Confirmed;

        var exception = new Exception("Database error");
        _orderConfirmedProcessMock.Setup(x => x.HandleOrder(message.OrderId, message.Status))
            .ThrowsAsync(exception);

        await Assert.ThrowsAsync<Exception>(() => _subscriber.HandleAsync(message));

        _orderConfirmedProcessMock.Verify(x => x.HandleOrder(message.OrderId, message.Status), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithDifferentStatuses_ShouldProcessCorrectly()
    {
        var message = AutoFaker.Generate<OrderConfirmed>();
        message.OrderId = Guid.NewGuid();
        message.Status = Sales.Application.Enums.Status.Cancelled;

        _orderConfirmedProcessMock.Setup(x => x.HandleOrder(message.OrderId, message.Status))
            .ReturnsAsync(new Sales.Application.ValueObject.Result<Sales.Application.Entities.Order>(true, "Success", AutoFaker.Generate<Sales.Application.Entities.Order>()));

        await _subscriber.HandleAsync(message);

        _orderConfirmedProcessMock.Verify(x => x.HandleOrder(message.OrderId, message.Status), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassTokenToProcess()
    {
        var message = AutoFaker.Generate<OrderConfirmed>();
        message.OrderId = Guid.NewGuid();
        message.Status = Sales.Application.Enums.Status.Confirmed;

        var cancellationToken = new CancellationToken();

        _orderConfirmedProcessMock.Setup(x => x.HandleOrder(message.OrderId, message.Status))
            .ReturnsAsync(new Sales.Application.ValueObject.Result<Sales.Application.Entities.Order>(true, "Success", AutoFaker.Generate<Sales.Application.Entities.Order>()));

        await _subscriber.HandleAsync(message, cancellationToken);

        _orderConfirmedProcessMock.Verify(x => x.HandleOrder(message.OrderId, message.Status), Times.Once);
    }
}
