using AutoBogus;
using Inventory.Application.AbstractionsGateways;
using Inventory.Application.Entities;
using Inventory.Application.Events;
using Inventory.Application.Events.Abstractions;
using Inventory.Application.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Inventory.Tests.ApplicationTestes.UseCasesTests;

public class ProcessOrderCreatedUnitTests
{
    private readonly Mock<IProductGateway> _mockProductGateway;
    private readonly Mock<ILogger<ProcessOrderCreated>> _mockLogger;
    private readonly Mock<IOrderConfirmedPublisher> _mockOrderConfirmedPublisher;
    private readonly ProcessOrderCreated _processOrderCreated;

    public ProcessOrderCreatedUnitTests()
    {
        _mockProductGateway = new Mock<IProductGateway>();
        _mockLogger = new Mock<ILogger<ProcessOrderCreated>>();
        _mockOrderConfirmedPublisher = new Mock<IOrderConfirmedPublisher>();
        
        _processOrderCreated = new ProcessOrderCreated(
            _mockProductGateway.Object,
            _mockLogger.Object,
            _mockOrderConfirmedPublisher.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidOrder_ShouldUpdateStockAndPublishEvent()
    {
        // Arrange
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        var product = AutoFaker.Generate<Product>();
        product.StockQuantity = 100;
        
        orderEvent.Items = new List<OrderItemEvent>
        {
            new OrderItemEvent
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Quantity = 10,
                UnitPrice = product.Price
            }
        };

        _mockProductGateway
            .Setup(x => x.GetProductById(product.ProductId))
            .ReturnsAsync(product);

        _mockProductGateway
            .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        await _processOrderCreated.ExecuteAsync(orderEvent);

        // Assert
        _mockProductGateway.Verify(x => x.GetProductById(product.ProductId), Times.Once);
        _mockProductGateway.Verify(x => x.UpdateQuantityProduct(90, product.ProductId), Times.Once);
        _mockOrderConfirmedPublisher.Verify(x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()), Times.Once);
        
        VerifyLogInformation("Message received in 'ProcessOrderCreated' use case:");
        VerifyLogInformation("StockUpdateConfirmedEvent publicado para produto");
        VerifyLogInformation("Message processed in 'ProcessOrderCreated' use case");
    }

    [Fact]
    public async Task ExecuteAsync_WithMultipleItems_ShouldProcessAllItems()
    {
        // Arrange
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        var product1 = AutoFaker.Generate<Product>();
        var product2 = AutoFaker.Generate<Product>();
        
        product1.StockQuantity = 50;
        product2.StockQuantity = 30;
        
        orderEvent.Items = new List<OrderItemEvent>
        {
            new OrderItemEvent
            {
                ProductId = product1.ProductId,
                ProductName = product1.Name,
                Quantity = 5,
                UnitPrice = product1.Price
            },
            new OrderItemEvent
            {
                ProductId = product2.ProductId,
                ProductName = product2.Name,
                Quantity = 10,
                UnitPrice = product2.Price
            }
        };

        _mockProductGateway
            .Setup(x => x.GetProductById(product1.ProductId))
            .ReturnsAsync(product1);
            
        _mockProductGateway
            .Setup(x => x.GetProductById(product2.ProductId))
            .ReturnsAsync(product2);

        _mockProductGateway
            .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        await _processOrderCreated.ExecuteAsync(orderEvent);

        // Assert
        _mockProductGateway.Verify(x => x.GetProductById(product1.ProductId), Times.Once);
        _mockProductGateway.Verify(x => x.GetProductById(product2.ProductId), Times.Once);
        _mockProductGateway.Verify(x => x.UpdateQuantityProduct(45, product1.ProductId), Times.Once);
        _mockProductGateway.Verify(x => x.UpdateQuantityProduct(20, product2.ProductId), Times.Once);
        _mockOrderConfirmedPublisher.Verify(x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentProduct_ShouldLogWarningAndPublishEvent()
    {
        // Arrange
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        var nonExistentProductId = Guid.NewGuid();
        
        orderEvent.Items = new List<OrderItemEvent>
        {
            new OrderItemEvent
            {
                ProductId = nonExistentProductId,
                ProductName = "Produto Inexistente",
                Quantity = 5,
                UnitPrice = 10.0m
            }
        };

        _mockProductGateway
            .Setup(x => x.GetProductById(nonExistentProductId))
            .ReturnsAsync((Product?)null);

        // Act
        await _processOrderCreated.ExecuteAsync(orderEvent);

        // Assert
        _mockProductGateway.Verify(x => x.GetProductById(nonExistentProductId), Times.Once);
        _mockProductGateway.Verify(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);
        _mockOrderConfirmedPublisher.Verify(x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()), Times.Once);
        
        VerifyLogWarning("Produto não encontrado:");
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroQuantity_ShouldProcessCorrectly()
    {
        // Arrange
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        var product = AutoFaker.Generate<Product>();
        product.StockQuantity = 100;
        
        orderEvent.Items = new List<OrderItemEvent>
        {
            new OrderItemEvent
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Quantity = 0,
                UnitPrice = product.Price
            }
        };

        _mockProductGateway
            .Setup(x => x.GetProductById(product.ProductId))
            .ReturnsAsync(product);

        _mockProductGateway
            .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        await _processOrderCreated.ExecuteAsync(orderEvent);

        // Assert
        _mockProductGateway.Verify(x => x.UpdateQuantityProduct(100, product.ProductId), Times.Once);
        _mockOrderConfirmedPublisher.Verify(x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithNegativeStockResult_ShouldStillUpdate()
    {
        // Arrange
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        var product = AutoFaker.Generate<Product>();
        product.StockQuantity = 5; // Estoque menor que quantidade solicitada
        
        orderEvent.Items = new List<OrderItemEvent>
        {
            new OrderItemEvent
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Quantity = 10, // Quantidade maior que estoque
                UnitPrice = product.Price
            }
        };

        _mockProductGateway
            .Setup(x => x.GetProductById(product.ProductId))
            .ReturnsAsync(product);

        _mockProductGateway
            .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        await _processOrderCreated.ExecuteAsync(orderEvent);

        // Assert
        _mockProductGateway.Verify(x => x.UpdateQuantityProduct(-5, product.ProductId), Times.Once);
        _mockOrderConfirmedPublisher.Verify(x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPublishCorrectStockUpdateEvent()
    {
        // Arrange
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        var product = AutoFaker.Generate<Product>();
        product.StockQuantity = 100;
        
        orderEvent.Items = new List<OrderItemEvent>
        {
            new OrderItemEvent
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Quantity = 15,
                UnitPrice = product.Price
            }
        };

        _mockProductGateway
            .Setup(x => x.GetProductById(product.ProductId))
            .ReturnsAsync(product);

        _mockProductGateway
            .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        StockUpdateConfirmedEvent? publishedEvent = null;
        _mockOrderConfirmedPublisher
            .Setup(x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()))
            .Callback<StockUpdateConfirmedEvent>(e => publishedEvent = e);

        // Act
        await _processOrderCreated.ExecuteAsync(orderEvent);

        // Assert
        Assert.NotNull(publishedEvent);
        Assert.Equal(orderEvent.OrderId, publishedEvent.OrderId);
        Assert.Equal(product.ProductId, publishedEvent.ProductId);
        Assert.Equal(product.Name, publishedEvent.ProductName);
        Assert.Equal(15, publishedEvent.QuantityReserved);
        Assert.Equal(15, publishedEvent.NewStockQuantity); // Soma das quantidades dos itens
        Assert.Equal("Confirmed", publishedEvent.Status);
        Assert.True(publishedEvent.ConfirmedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task ExecuteAsync_WithGatewayException_ShouldPropagateException()
    {
        // Arrange
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        var product = AutoFaker.Generate<Product>();
        
        orderEvent.Items = new List<OrderItemEvent>
        {
            new OrderItemEvent
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Quantity = 5,
                UnitPrice = product.Price
            }
        };

        _mockProductGateway
            .Setup(x => x.GetProductById(product.ProductId))
            .ReturnsAsync(product);

        _mockProductGateway
            .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _processOrderCreated.ExecuteAsync(orderEvent));
        
        _mockProductGateway.Verify(x => x.GetProductById(product.ProductId), Times.Once);
        _mockProductGateway.Verify(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyItemsList_ShouldCompleteWithoutErrors()
    {
        // Arrange
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        orderEvent.Items = new List<OrderItemEvent>();

        // Act
        await _processOrderCreated.ExecuteAsync(orderEvent);

        // Assert
        _mockProductGateway.Verify(x => x.GetProductById(It.IsAny<Guid>()), Times.Never);
        _mockProductGateway.Verify(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()), Times.Never);
        _mockOrderConfirmedPublisher.Verify(x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()), Times.Never);
        
        VerifyLogInformation("Message received in 'ProcessOrderCreated' use case:");
        VerifyLogInformation("Message processed in 'ProcessOrderCreated' use case");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldCalculateNewStockQuantityCorrectly()
    {
        // Arrange
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        var product = AutoFaker.Generate<Product>();
        product.StockQuantity = 100;
        
        orderEvent.Items = new List<OrderItemEvent>
        {
            new OrderItemEvent
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Quantity = 20,
                UnitPrice = product.Price
            },
            new OrderItemEvent
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                Quantity = 30,
                UnitPrice = product.Price
            }
        };

        _mockProductGateway
            .Setup(x => x.GetProductById(product.ProductId))
            .ReturnsAsync(product);

        _mockProductGateway
            .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var publishedEvents = new List<StockUpdateConfirmedEvent>();
        _mockOrderConfirmedPublisher
            .Setup(x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()))
            .Callback<StockUpdateConfirmedEvent>(e => publishedEvents.Add(e));

        // Act
        await _processOrderCreated.ExecuteAsync(orderEvent);

        // Assert
        Assert.Equal(2, publishedEvents.Count);
        
        // NewStockQuantity deve ser a soma de todas as quantidades (20 + 30 = 50)
        foreach (var publishedEvent in publishedEvents)
        {
            Assert.Equal(50, publishedEvent.NewStockQuantity);
        }
    }

    private void VerifyLogInformation(string message)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    private void VerifyLogWarning(string message)
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
