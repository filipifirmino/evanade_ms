using AutoBogus;
using Microsoft.Extensions.Logging;
using Moq;
using Sales.Application.AbstractionsGateways;
using Sales.Application.Entities;
using Sales.Application.Enums;
using Sales.Application.UseCases;
using Sales.Application.UseCases.Abstractions;
using Sales.Application.ValueObject;

namespace Sales.Tests.ApplicationTestes.UseCasesTests;

public class OrderConfirmedProcessTests
{
    private readonly Mock<IOrderGateway> _orderGatewayMock;
    private readonly Mock<ILogger<OrderConfirmedProcess>> _loggerMock;
    private readonly OrderConfirmedProcess _orderConfirmedProcess;

    public OrderConfirmedProcessTests()
    {
        _orderGatewayMock = new Mock<IOrderGateway>();
        _loggerMock = new Mock<ILogger<OrderConfirmedProcess>>();
        
        _orderConfirmedProcess = new OrderConfirmedProcess(
            _orderGatewayMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleOrder_WithValidOrderId_ShouldUpdateOrderStatus()
    {
        var orderId = Guid.NewGuid();
        var status = Status.Confirmed;

        _orderGatewayMock.Setup(x => x.UpdateOrderStatus(orderId, status))
            .Returns(Task.CompletedTask);

        var result = await _orderConfirmedProcess.HandleOrder(orderId, status);

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        
        _orderGatewayMock.Verify(x => x.UpdateOrderStatus(orderId, status), Times.Once);
    }

    [Fact]
    public async Task HandleOrder_WhenGatewayThrowsException_ShouldThrowException()
    {
        var orderId = Guid.NewGuid();
        var status = Status.Confirmed;

        _orderGatewayMock.Setup(x => x.UpdateOrderStatus(orderId, status))
            .ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() => 
            _orderConfirmedProcess.HandleOrder(orderId, status));
        
        _orderGatewayMock.Verify(x => x.UpdateOrderStatus(orderId, status), Times.Once);
    }

    [Theory]
    [InlineData(Status.Confirmed)]
    [InlineData(Status.Cancelled)]
    public async Task HandleOrder_WithDifferentStatuses_ShouldUpdateCorrectly(Status status)
    {
        var orderId = Guid.NewGuid();

        _orderGatewayMock.Setup(x => x.UpdateOrderStatus(orderId, status))
            .Returns(Task.CompletedTask);

        var result = await _orderConfirmedProcess.HandleOrder(orderId, status);

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        
        _orderGatewayMock.Verify(x => x.UpdateOrderStatus(orderId, status), Times.Once);
    }

    [Fact]
    public async Task HandleOrder_ShouldLogInformation()
    {
        var orderId = Guid.NewGuid();
        var status = Status.Confirmed;

        _orderGatewayMock.Setup(x => x.UpdateOrderStatus(orderId, status))
            .Returns(Task.CompletedTask);

        await _orderConfirmedProcess.HandleOrder(orderId, status);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Handling order confirmation")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
