using AutoBogus;
using Microsoft.Extensions.Logging;
using Moq;
using Sales.Application.AbstractionsGateways;
using Sales.Application.AbstractionRabbit;
using Sales.Application.Entities;
using Sales.Application.Events;
using Sales.Application.UseCases;
using Sales.Application.UseCases.Abstractions;
using Sales.Application.ValueObject;

namespace Sales.Tests.ApplicationTestes.UseCasesTests;

public class OrderProcessTests
{
    private readonly Mock<IOrderGateway> _orderGatewayMock;
    private readonly Mock<IHttpGateway> _httpGatewayMock;
    private readonly Mock<IGenericEventProducer> _eventProducerMock;
    private readonly OrderProcess _orderProcess;

    public OrderProcessTests()
    {
        _orderGatewayMock = new Mock<IOrderGateway>();
        var loggerMock = new Mock<ILogger<OrderProcess>>();
        _httpGatewayMock = new Mock<IHttpGateway>();
        _eventProducerMock = new Mock<IGenericEventProducer>();

        _orderProcess = new OrderProcess(
            _orderGatewayMock.Object,
            loggerMock.Object,
            _httpGatewayMock.Object,
            _eventProducerMock.Object);
    }

    [Fact]
    public async Task HandleOrder_WithInsufficientStock_ShouldReturnFailure()
    {
        var order = AutoFaker.Generate<Order>();
        order.Items = new List<OrderItem>
        {
            new OrderItem(Guid.NewGuid(), 10, 50.0m)
        };

        _httpGatewayMock.Setup(x => x.GetProductStockQuantity(It.IsAny<Guid>(), 
                It.IsAny<string>()))
            .ReturnsAsync(5); 

        var result = await _orderProcess.HandleOrder(order);

        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains("Insufficient stock", result.Message);
        
        _orderGatewayMock.Verify(x => x.AddProduct(It.IsAny<Order>()), Times.Never);
        _eventProducerMock.Verify(x => x.PublishEventAsync(It.IsAny<OrderCreated>(), 
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleOrder_WithDatabaseError_ShouldReturnFailure()
    {
        var order = AutoFaker.Generate<Order>();

        _httpGatewayMock.Setup(x => x.GetProductStockQuantity(It.IsAny<Guid>(), 
                It.IsAny<string>()))
            .ReturnsAsync(100);

        _orderGatewayMock.Setup(x => x.AddProduct(It.IsAny<Order>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _orderProcess.HandleOrder(order);

        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task HandleOrder_WithMultipleItems_ShouldCheckStockForAllItems()
    {
        var order = AutoFaker.Generate<Order>();
        order.Items = new List<OrderItem>
        {
            new OrderItem(Guid.NewGuid(), 5, 50.0m),
            new OrderItem(Guid.NewGuid(), 3, 30.0m)
        };

        _httpGatewayMock.Setup(x => x.GetProductStockQuantity(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(100);

        var processedOrder = AutoFaker.Generate<Order>();
        _orderGatewayMock.Setup(x => x.AddProduct(It.IsAny<Order>()))
            .ReturnsAsync(processedOrder);

        _eventProducerMock.Setup(x => x.PublishEventAsync(It.IsAny<OrderCreated>(), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _orderProcess.HandleOrder(order);

        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        
        _httpGatewayMock.Verify(x => x.GetProductStockQuantity(It.IsAny<Guid>(), It.IsAny<string>()), Times.Exactly(2));
        _orderGatewayMock.Verify(x => x.AddProduct(It.IsAny<Order>()), Times.Once);
        _eventProducerMock.Verify(x => x.PublishEventAsync(It.IsAny<OrderCreated>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}