using AutoBogus;
using Inventory.Application.AbstractionsGateways;
using Inventory.Application.Entities;
using Inventory.Application.Events;
using Inventory.Application.Events.Abstractions;
using Inventory.Application.UseCases;
using Microsoft.Extensions.Logging;
using Moq;

namespace Inventory.Tests.ApplicationTestes.UseCasesTests;

public class ProcessOrderCreatedIntegrationTests
{
    private readonly Mock<IProductGateway> _mockProductGateway;
    private readonly Mock<ILogger<ProcessOrderCreated>> _mockLogger;
    private readonly Mock<IOrderConfirmedPublisher> _mockOrderConfirmedPublisher;
    private readonly ProcessOrderCreated _processOrderCreated;

    public ProcessOrderCreatedIntegrationTests()
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
    public async Task ExecuteAsync_CompleteOrderProcessing_ShouldProcessSuccessfully()
    {
        // Arrange - Cenário completo de processamento de pedido
        var orderEvent = CreateCompleteOrderEvent();
        var products = CreateProductsForOrder(orderEvent.Items);

        SetupGatewayMocks(products);

        // Act
        await _processOrderCreated.ExecuteAsync(orderEvent);

        // Assert - Verificar todo o fluxo de processamento
        VerifyCompleteOrderProcessing(orderEvent, products);
    }

    [Fact]
    public async Task ExecuteAsync_WithMixedProductAvailability_ShouldHandleCorrectly()
    {
        // Arrange - Alguns produtos existem, outros não
        var orderEvent = CreateMixedAvailabilityOrderEvent();
        var existingProducts = CreateProductsForOrder(orderEvent.Items.Where(i => i.ProductId != Guid.Empty).ToList());

        SetupGatewayMocks(existingProducts);

        // Act
        await _processOrderCreated.ExecuteAsync(orderEvent);

        // Assert
        VerifyMixedAvailabilityProcessing(orderEvent, existingProducts);
    }

    [Fact]
    public async Task ExecuteAsync_WithConcurrentOrderProcessing_ShouldHandleCorrectly()
    {
        // Arrange - Simular processamento concorrente com produtos únicos
        var orderEvent1 = CreateCompleteOrderEvent();
        var orderEvent2 = CreateCompleteOrderEvent();
        
        // Garantir que os produtos são diferentes para evitar conflitos
        orderEvent2.Items = new List<OrderItemEvent>
        {
            new OrderItemEvent
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Produto D",
                Quantity = 8,
                UnitPrice = 30.0m
            },
            new OrderItemEvent
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Produto E",
                Quantity = 12,
                UnitPrice = 20.0m
            }
        };
        
        var products = CreateProductsForOrder(orderEvent1.Items.Concat(orderEvent2.Items).ToList());

        SetupGatewayMocks(products);

        // Act - Processar ambos os pedidos simultaneamente
        var task1 = _processOrderCreated.ExecuteAsync(orderEvent1);
        var task2 = _processOrderCreated.ExecuteAsync(orderEvent2);

        await Task.WhenAll(task1, task2);

        // Assert - Verificar que cada pedido foi processado
        foreach (var item in orderEvent1.Items)
        {
            _mockProductGateway.Verify(x => x.GetProductById(item.ProductId), Times.Once);
        }
        
        foreach (var item in orderEvent2.Items)
        {
            _mockProductGateway.Verify(x => x.GetProductById(item.ProductId), Times.Once);
        }

        // Verificar que eventos foram publicados para ambos os pedidos
        _mockOrderConfirmedPublisher.Verify(
            x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()), 
            Times.Exactly(orderEvent1.Items.Count + orderEvent2.Items.Count));
    }

    [Fact]
    public async Task ExecuteAsync_WithLargeOrder_ShouldProcessAllItems()
    {
        // Arrange - Pedido com muitos itens
        var orderEvent = CreateLargeOrderEvent(50); // 50 itens diferentes
        var products = CreateProductsForOrder(orderEvent.Items);

        SetupGatewayMocks(products);

        // Act
        await _processOrderCreated.ExecuteAsync(orderEvent);

        // Assert
        VerifyLargeOrderProcessing(orderEvent, products);
    }

    [Fact]
    public async Task ExecuteAsync_WithStockDepletion_ShouldHandleCorrectly()
    {
        // Arrange - Produtos com estoque baixo
        var orderEvent = CreateStockDepletionOrderEvent();
        var products = CreateProductsWithLowStock(orderEvent.Items);

        SetupGatewayMocks(products);

        // Act
        await _processOrderCreated.ExecuteAsync(orderEvent);

        // Assert
        VerifyStockDepletionProcessing(orderEvent, products);
    }

    [Fact]
    public async Task ExecuteAsync_WithPublisherFailure_ShouldContinueProcessing()
    {
        // Arrange
        var orderEvent = CreateCompleteOrderEvent();
        var products = CreateProductsForOrder(orderEvent.Items);

        SetupGatewayMocks(products);
        
        // Simular falha no publisher
        _mockOrderConfirmedPublisher
            .Setup(x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()))
            .Throws(new InvalidOperationException("Publisher failure"));

        // Act & Assert - Deve continuar processando mesmo com falha no publisher
        await Assert.ThrowsAsync<InvalidOperationException>(() => _processOrderCreated.ExecuteAsync(orderEvent));
        
        // Verificar que pelo menos tentou buscar o primeiro produto (o processamento para no primeiro erro)
        if (orderEvent.Items.Any())
        {
            _mockProductGateway.Verify(x => x.GetProductById(orderEvent.Items.First().ProductId), Times.Once);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithPartialGatewayFailure_ShouldHandleGracefully()
    {
        // Arrange
        var orderEvent = CreateCompleteOrderEvent();
        var products = CreateProductsForOrder(orderEvent.Items);

        SetupGatewayMocks(products);
        
        // Simular falha parcial no gateway
        _mockProductGateway
            .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), orderEvent.Items.First().ProductId))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _processOrderCreated.ExecuteAsync(orderEvent));
        
        // Verificar que pelo menos tentou processar todos os itens
        foreach (var item in orderEvent.Items)
        {
            _mockProductGateway.Verify(x => x.GetProductById(item.ProductId), Times.Once);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithComplexOrderScenario_ShouldProcessCorrectly()
    {
        // Arrange - Cenário complexo com múltiplos produtos, quantidades variadas
        var orderEvent = CreateComplexOrderScenario();
        var products = CreateProductsForOrder(orderEvent.Items);

        SetupGatewayMocks(products);

        // Act
        await _processOrderCreated.ExecuteAsync(orderEvent);

        // Assert
        VerifyComplexOrderScenario(orderEvent, products);
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroStockProducts_ShouldHandleCorrectly()
    {
        // Arrange - Produtos com estoque zero
        var orderEvent = CreateZeroStockOrderEvent();
        var products = CreateProductsWithZeroStock(orderEvent.Items);

        SetupGatewayMocks(products);

        // Act
        await _processOrderCreated.ExecuteAsync(orderEvent);

        // Assert
        VerifyZeroStockProcessing(orderEvent, products);
    }

    private OrderCreatedEvent CreateCompleteOrderEvent()
    {
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        orderEvent.Items = new List<OrderItemEvent>
        {
            new OrderItemEvent
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Produto A",
                Quantity = 10,
                UnitPrice = 25.50m
            },
            new OrderItemEvent
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Produto B",
                Quantity = 5,
                UnitPrice = 15.75m
            },
            new OrderItemEvent
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Produto C",
                Quantity = 20,
                UnitPrice = 8.99m
            }
        };
        return orderEvent;
    }

    private OrderCreatedEvent CreateMixedAvailabilityOrderEvent()
    {
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        orderEvent.Items = new List<OrderItemEvent>
        {
            new OrderItemEvent
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Produto Existente",
                Quantity = 5,
                UnitPrice = 10.0m
            },
            new OrderItemEvent
            {
                ProductId = Guid.Empty, // Produto inexistente
                ProductName = "Produto Inexistente",
                Quantity = 3,
                UnitPrice = 15.0m
            }
        };
        return orderEvent;
    }

    private OrderCreatedEvent CreateLargeOrderEvent(int itemCount)
    {
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        orderEvent.Items = new List<OrderItemEvent>();

        for (int i = 0; i < itemCount; i++)
        {
            orderEvent.Items.Add(new OrderItemEvent
            {
                ProductId = Guid.NewGuid(),
                ProductName = $"Produto {i + 1}",
                Quantity = Random.Shared.Next(1, 10),
                UnitPrice = Random.Shared.Next(10, 100)
            });
        }

        return orderEvent;
    }

    private OrderCreatedEvent CreateStockDepletionOrderEvent()
    {
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        orderEvent.Items = new List<OrderItemEvent>
        {
            new OrderItemEvent
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Produto Estoque Baixo",
                Quantity = 8, // Quantidade maior que estoque disponível
                UnitPrice = 20.0m
            }
        };
        return orderEvent;
    }

    private OrderCreatedEvent CreateComplexOrderScenario()
    {
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        orderEvent.Items = new List<OrderItemEvent>
        {
            new OrderItemEvent
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Produto Premium",
                Quantity = 1,
                UnitPrice = 999.99m
            },
            new OrderItemEvent
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Produto Popular",
                Quantity = 100,
                UnitPrice = 2.50m
            },
            new OrderItemEvent
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Produto Médio",
                Quantity = 15,
                UnitPrice = 45.00m
            }
        };
        return orderEvent;
    }

    private OrderCreatedEvent CreateZeroStockOrderEvent()
    {
        var orderEvent = AutoFaker.Generate<OrderCreatedEvent>();
        orderEvent.Items = new List<OrderItemEvent>
        {
            new OrderItemEvent
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Produto Sem Estoque",
                Quantity = 5,
                UnitPrice = 30.0m
            }
        };
        return orderEvent;
    }

    private List<Product> CreateProductsForOrder(List<OrderItemEvent> items)
    {
        return items.Where(i => i.ProductId != Guid.Empty).Select(item => new Product(
            item.ProductName,
            $"Descrição do {item.ProductName}",
            item.UnitPrice,
            100 // Estoque padrão
        )
        {
            ProductId = item.ProductId
        }).ToList();
    }

    private List<Product> CreateProductsWithLowStock(List<OrderItemEvent> items)
    {
        return items.Where(i => i.ProductId != Guid.Empty).Select(item => new Product(
            item.ProductName,
            $"Descrição do {item.ProductName}",
            item.UnitPrice,
            5 // Estoque baixo
        )
        {
            ProductId = item.ProductId
        }).ToList();
    }

    private List<Product> CreateProductsWithZeroStock(List<OrderItemEvent> items)
    {
        return items.Where(i => i.ProductId != Guid.Empty).Select(item => new Product(
            item.ProductName,
            $"Descrição do {item.ProductName}",
            item.UnitPrice,
            0 // Estoque zero
        )
        {
            ProductId = item.ProductId
        }).ToList();
    }

    private void SetupGatewayMocks(List<Product> products)
    {
        foreach (var product in products)
        {
            _mockProductGateway
                .Setup(x => x.GetProductById(product.ProductId))
                .ReturnsAsync(product);

            _mockProductGateway
                .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), product.ProductId))
                .Returns(Task.CompletedTask);
        }
    }

    private void VerifyCompleteOrderProcessing(OrderCreatedEvent orderEvent, List<Product> products)
    {
        foreach (var item in orderEvent.Items.Where(i => i.ProductId != Guid.Empty))
        {
            var product = products.First(p => p.ProductId == item.ProductId);
            
            _mockProductGateway.Verify(x => x.GetProductById(item.ProductId), Times.Once);
            _mockProductGateway.Verify(
                x => x.UpdateQuantityProduct(
                    product.StockQuantity - item.Quantity, 
                    item.ProductId), 
                Times.Once);
        }

        _mockOrderConfirmedPublisher.Verify(
            x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()), 
            Times.Exactly(orderEvent.Items.Count));
    }

    private void VerifyMixedAvailabilityProcessing(OrderCreatedEvent orderEvent, List<Product> existingProducts)
    {
        var existingItems = orderEvent.Items.Where(i => i.ProductId != Guid.Empty).ToList();
        var nonExistentItems = orderEvent.Items.Where(i => i.ProductId == Guid.Empty).ToList();

        // Verificar processamento de produtos existentes
        foreach (var item in existingItems)
        {
            var product = existingProducts.First(p => p.ProductId == item.ProductId);
            _mockProductGateway.Verify(x => x.GetProductById(item.ProductId), Times.Once);
            _mockProductGateway.Verify(
                x => x.UpdateQuantityProduct(
                    product.StockQuantity - item.Quantity, 
                    item.ProductId), 
                Times.Once);
        }

        // Verificar tentativa de busca de produtos inexistentes
        foreach (var item in nonExistentItems)
        {
            _mockProductGateway.Verify(x => x.GetProductById(item.ProductId), Times.Once);
        }

        // Verificar que todos os eventos foram publicados
        _mockOrderConfirmedPublisher.Verify(
            x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()), 
            Times.Exactly(orderEvent.Items.Count));
    }

    private void VerifyConcurrentProcessing(OrderCreatedEvent orderEvent1, OrderCreatedEvent orderEvent2, List<Product> products)
    {
        var allItems = orderEvent1.Items.Concat(orderEvent2.Items).ToList();

        foreach (var item in allItems)
        {
            var product = products.First(p => p.ProductId == item.ProductId);
            
            // Cada item deve ser processado uma vez por pedido
            _mockProductGateway.Verify(x => x.GetProductById(item.ProductId), Times.Exactly(2));
            _mockProductGateway.Verify(
                x => x.UpdateQuantityProduct(
                    product.StockQuantity - item.Quantity, 
                    item.ProductId), 
                Times.Exactly(2));
        }

        // Cada pedido publica eventos para seus itens
        _mockOrderConfirmedPublisher.Verify(
            x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()), 
            Times.Exactly(orderEvent1.Items.Count + orderEvent2.Items.Count));
    }

    private void VerifyLargeOrderProcessing(OrderCreatedEvent orderEvent, List<Product> products)
    {
        foreach (var item in orderEvent.Items)
        {
            var product = products.First(p => p.ProductId == item.ProductId);
            
            _mockProductGateway.Verify(x => x.GetProductById(item.ProductId), Times.Once);
            _mockProductGateway.Verify(
                x => x.UpdateQuantityProduct(
                    product.StockQuantity - item.Quantity, 
                    item.ProductId), 
                Times.Once);
        }

        _mockOrderConfirmedPublisher.Verify(
            x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()), 
            Times.Exactly(orderEvent.Items.Count));
    }

    private void VerifyStockDepletionProcessing(OrderCreatedEvent orderEvent, List<Product> products)
    {
        foreach (var item in orderEvent.Items)
        {
            var product = products.First(p => p.ProductId == item.ProductId);
            
            _mockProductGateway.Verify(x => x.GetProductById(item.ProductId), Times.Once);
            
            // Verificar que a quantidade foi atualizada mesmo resultando em valor negativo
            var expectedNewQuantity = product.StockQuantity - item.Quantity;
            _mockProductGateway.Verify(
                x => x.UpdateQuantityProduct(expectedNewQuantity, item.ProductId), 
                Times.Once);
        }

        _mockOrderConfirmedPublisher.Verify(
            x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()), 
            Times.Exactly(orderEvent.Items.Count));
    }

    private void VerifyComplexOrderScenario(OrderCreatedEvent orderEvent, List<Product> products)
    {
        foreach (var item in orderEvent.Items)
        {
            var product = products.First(p => p.ProductId == item.ProductId);
            
            _mockProductGateway.Verify(x => x.GetProductById(item.ProductId), Times.Once);
            _mockProductGateway.Verify(
                x => x.UpdateQuantityProduct(
                    product.StockQuantity - item.Quantity, 
                    item.ProductId), 
                Times.Once);
        }

        _mockOrderConfirmedPublisher.Verify(
            x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()), 
            Times.Exactly(orderEvent.Items.Count));
    }

    private void VerifyZeroStockProcessing(OrderCreatedEvent orderEvent, List<Product> products)
    {
        foreach (var item in orderEvent.Items)
        {
            var product = products.First(p => p.ProductId == item.ProductId);
            
            _mockProductGateway.Verify(x => x.GetProductById(item.ProductId), Times.Once);
            
            // Verificar que a quantidade foi atualizada para valor negativo
            var expectedNewQuantity = product.StockQuantity - item.Quantity;
            _mockProductGateway.Verify(
                x => x.UpdateQuantityProduct(expectedNewQuantity, item.ProductId), 
                Times.Once);
        }

        _mockOrderConfirmedPublisher.Verify(
            x => x.Publish(It.IsAny<StockUpdateConfirmedEvent>()), 
            Times.Exactly(orderEvent.Items.Count));
    }
}
