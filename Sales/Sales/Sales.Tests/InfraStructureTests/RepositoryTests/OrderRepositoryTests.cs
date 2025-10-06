using AutoBogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Sales.Infrastructure.Configure;
using Sales.Infrastructure.Entities;
using Sales.Infrastructure.Repositories;
using Sales.Infrastructure.Repositories.Abstractions;

namespace Sales.Tests.InfraStructureTests.RepositoryTests;

public class OrderRepositoryTests : IDisposable
{
    private readonly DataContext _context;
    private readonly OrderRepository _repository;

    public OrderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockConfiguration = new Mock<IConfiguration>();
        _context = new DataContext(options, mockConfiguration.Object);
        _repository = new OrderRepository(_context);
    }

    [Fact]
    public async Task AddOrder_WithValidOrder_ShouldAddToDatabase()
    {
        var orderEntity = AutoFaker.Generate<OrderEntity>();
        orderEntity.OrderId = Guid.NewGuid();

        await _repository.AddAsync(orderEntity);
        await _context.SaveChangesAsync();

        var savedOrder = await _context.Set<OrderEntity>()
            .FirstOrDefaultAsync(x => x.OrderId == orderEntity.OrderId);
        
        Assert.NotNull(savedOrder);
        Assert.Equal(orderEntity.OrderId, savedOrder.OrderId);
        Assert.Equal(orderEntity.CustomerId, savedOrder.CustomerId);
        Assert.Equal(orderEntity.TotalAmount, savedOrder.TotalAmount);
    }

    [Fact]
    public async Task GetOrderById_WithValidId_ShouldReturnOrder()
    {
        var orderEntity = AutoFaker.Generate<OrderEntity>();
        orderEntity.OrderId = Guid.NewGuid();
        
        _context.Set<OrderEntity>().Add(orderEntity);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(orderEntity.OrderId);

        Assert.NotNull(result);
        Assert.Equal(orderEntity.OrderId, result.OrderId);
        Assert.Equal(orderEntity.CustomerId, result.CustomerId);
        Assert.Equal(orderEntity.TotalAmount, result.TotalAmount);
    }

    [Fact]
    public async Task GetOrderById_WithNonExistentId_ShouldReturnNull()
    {
        var nonExistentId = Guid.NewGuid();

        var result = await _repository.GetByIdAsync(nonExistentId);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithValidId_ShouldUpdateStatus()
    {
        var orderEntity = AutoFaker.Generate<OrderEntity>();
        orderEntity.OrderId = Guid.NewGuid();
        orderEntity.Status = Sales.Application.Enums.Status.Created;
        
        _context.Set<OrderEntity>().Add(orderEntity);
        await _context.SaveChangesAsync();

        var newStatus = Sales.Application.Enums.Status.Confirmed;

        await _repository.UpdateOrderStatus(orderEntity.OrderId, newStatus);
        await _context.SaveChangesAsync();

        var updatedOrder = await _context.Set<OrderEntity>()
            .FirstAsync(x => x.OrderId == orderEntity.OrderId);
        
        Assert.Equal(newStatus, updatedOrder.Status);
    }

    [Fact]
    public async Task UpdateOrderStatus_WithNonExistentId_ShouldNotThrow()
    {
        var nonExistentId = Guid.NewGuid();
        var newStatus = Sales.Application.Enums.Status.Confirmed;

        await _repository.UpdateOrderStatus(nonExistentId, newStatus);
    }

    [Fact]
    public async Task GetAllOrders_ShouldReturnAllOrders()
    {
        var orders = AutoFaker.Generate<OrderEntity>(3);
        foreach (var order in orders)
        {
            order.OrderId = Guid.NewGuid();
            _context.Set<OrderEntity>().Add(order);
        }
        await _context.SaveChangesAsync();

        var result = await _repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAllOrders_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        var result = await _repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
