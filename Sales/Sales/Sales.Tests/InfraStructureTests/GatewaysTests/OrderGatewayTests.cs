using AutoBogus;
using Microsoft.Extensions.Logging;
using Moq;
using Sales.Application.AbstractionsGateways;
using Sales.Application.Entities;
using Sales.Application.Enums;
using Sales.Infrastructure.Gateways;
using Sales.Infrastructure.Repositories.Abstractions;

namespace Sales.Tests.InfraStructureTests.GatewaysTests;

public class OrderGatewayTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly OrderGateway _orderGateway;

    public OrderGatewayTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _orderGateway = new OrderGateway(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task AddProduct_WithValidOrder_ShouldReturnOrder()
    {
        var order = AutoFaker.Generate<Order>();
        var orderEntity = AutoFaker.Generate<Sales.Infrastructure.Entities.OrderEntity>();

        _orderRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Sales.Infrastructure.Entities.OrderEntity>()))
            .ReturnsAsync(orderEntity);

        var result = await _orderGateway.AddProduct(order);

        Assert.NotNull(result);
        _orderRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Sales.Infrastructure.Entities.OrderEntity>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidId_ShouldCallRepository()
    {
        var orderId = Guid.NewGuid();
        var status = Status.Confirmed;

        _orderRepositoryMock.Setup(x => x.UpdateOrderStatus(orderId, status))
            .Returns(Task.CompletedTask);

        await _orderGateway.UpdateOrderStatus(orderId, status);

        _orderRepositoryMock.Verify(x => x.UpdateOrderStatus(orderId, status), Times.Once);
    }

    [Fact]
    public async Task GetOrderById_WithValidId_ShouldReturnOrder()
    {
        var orderId = Guid.NewGuid();
        var orderEntity = AutoFaker.Generate<Sales.Infrastructure.Entities.OrderEntity>();
        orderEntity.OrderId = orderId;

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(orderId))
            .ReturnsAsync(orderEntity);

        var result = await _orderGateway.GetOrderById(orderId);

        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        _orderRepositoryMock.Verify(x => x.GetByIdAsync(orderId), Times.Once);
    }
}