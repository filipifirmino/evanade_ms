using AutoBogus;
using Inventory.Application.Entities;
using Inventory.InfraStructure.Entities;
using Inventory.InfraStructure.Gateways;
using Inventory.InfraStructure.Repositories.Abstractions;
using Inventory.InfraStructure.Tools;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;

namespace Inventory.Tests.InfraStructureTests.GatewaysTests;

public class ProductGatewayIntegrationTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<ILogger<ProductGateway>> _mockLogger;
    private readonly ProductGateway _productGateway;

    public ProductGatewayIntegrationTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductGateway>>();
        _productGateway = new ProductGateway(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CompleteProductLifecycle_ShouldWorkCorrectly()
    {
        // Arrange - Criar produto
        var product = AutoFaker.Generate<Product>();
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = product.ProductId;
        productEntity.Name = product.Name;
        productEntity.Description = product.Description;
        productEntity.Price = product.Price;
        productEntity.StockQuantity = product.StockQuantity;

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<ProductEntity>()))
            .ReturnsAsync(productEntity);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ProductEntity>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(productEntity);

        _mockRepository
            .Setup(x => x.DeleteAsync(It.IsAny<ProductEntity>()))
            .Returns(Task.CompletedTask);

        // Act & Assert - Ciclo completo de vida do produto
        // 1. Criar produto
        var createdProduct = await _productGateway.AddProduct(product);
        Assert.NotNull(createdProduct);
        Assert.Equal(product.ProductId, createdProduct.ProductId);

        // 2. Buscar produto por ID
        var retrievedProduct = await _productGateway.GetProductById(product.ProductId);
        Assert.NotNull(retrievedProduct);
        Assert.Equal(product.ProductId, retrievedProduct.ProductId);

        // 3. Atualizar produto
        var updatedProduct = new Product("Updated Name", "Updated Description", 199.99m, 150);
        updatedProduct.ProductId = product.ProductId;
        await _productGateway.UpdateProduct(updatedProduct);

        // 4. Atualizar quantidade
        await _productGateway.UpdateQuantityProduct(200, product.ProductId);

        // 5. Excluir produto
        await _productGateway.DeleteProduct(updatedProduct);

        // Verificar que todas as operações foram chamadas
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<ProductEntity>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<ProductEntity>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<ProductEntity>()), Times.Once);
    }

    [Fact]
    public async Task BulkOperations_ShouldHandleMultipleProductsCorrectly()
    {
        // Arrange - Criar múltiplos produtos
        var products = AutoFaker.Generate<Product>(5);
        var productEntities = products.Select(p => new ProductEntity
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            Reservation = p.Reservation
        }).ToList();

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<ProductEntity>()))
            .ReturnsAsync((ProductEntity entity) => entity);

        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(productEntities);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ProductEntity>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.DeleteAsync(It.IsAny<ProductEntity>()))
            .Returns(Task.CompletedTask);

        // Act - Operações em lote
        var createdProducts = new List<Product>();
        foreach (var product in products)
        {
            var created = await _productGateway.AddProduct(product);
            createdProducts.Add(created);
        }

        var allProducts = await _productGateway.GetAllProducts();
        var allProductsList = allProducts.ToList();

        // Atualizar todos os produtos
        foreach (var product in allProductsList)
        {
            var updatedProduct = new Product("Bulk Updated", "Bulk Description", product.Price + 10, product.StockQuantity);
            updatedProduct.ProductId = product.ProductId;
            await _productGateway.UpdateProduct(updatedProduct);
        }

        // Excluir metade dos produtos
        for (int i = 0; i < allProductsList.Count / 2; i++)
        {
            await _productGateway.DeleteProduct(allProductsList[i]);
        }

        // Assert
        Assert.Equal(5, createdProducts.Count);
        Assert.Equal(5, allProductsList.Count);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<ProductEntity>()), Times.Exactly(5));
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<ProductEntity>()), Times.Exactly(5));
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<ProductEntity>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ConcurrentOperations_ShouldHandleCorrectly()
    {
        // Arrange
        var product = AutoFaker.Generate<Product>();
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = product.ProductId;

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<ProductEntity>()))
            .ReturnsAsync(productEntity);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ProductEntity>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(productEntity);

        _mockRepository
            .Setup(x => x.DeleteAsync(It.IsAny<ProductEntity>()))
            .Returns(Task.CompletedTask);

        // Act - Operações concorrentes
        var tasks = new List<Task>
        {
            _productGateway.AddProduct(product),
            _productGateway.UpdateProduct(product),
            _productGateway.UpdateQuantityProduct(100, product.ProductId),
            _productGateway.GetProductById(product.ProductId),
            _productGateway.DeleteProduct(product)
        };

        await Task.WhenAll(tasks);

        // Assert
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<ProductEntity>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<ProductEntity>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<ProductEntity>()), Times.Once);
    }


    [Fact]
    public async Task PerformanceTest_WithLargeDataset_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var products = AutoFaker.Generate<Product>(1000);
        var productEntities = products.Select(p => new ProductEntity
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            Reservation = p.Reservation
        }).ToList();

        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(productEntities);

        _mockRepository
            .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act
        var startTime = DateTime.UtcNow;
        
        var allProducts = await _productGateway.GetAllProducts();
        var allProductsList = allProducts.ToList();

        // Atualizar quantidade de todos os produtos
        var updateTasks = allProductsList.Select(p => 
            _productGateway.UpdateQuantityProduct(p.StockQuantity + 10, p.ProductId));
        
        await Task.WhenAll(updateTasks);
        
        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Assert
        Assert.Equal(1000, allProductsList.Count);
        Assert.True(duration.TotalSeconds < 5, $"Operação demorou {duration.TotalSeconds} segundos, esperado menos de 5 segundos");

        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
        _mockRepository.Verify(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()), Times.Exactly(1000));
    }

    [Fact]
    public async Task DataConsistency_WithComplexOperations_ShouldMaintainIntegrity()
    {
        // Arrange
        var product = AutoFaker.Generate<Product>();
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = product.ProductId;

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<ProductEntity>()))
            .ReturnsAsync(productEntity);

        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(productEntity);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ProductEntity>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        // Act - Operações complexas que devem manter consistência
        var createdProduct = await _productGateway.AddProduct(product);
        
        // Verificar se o produto foi criado corretamente
        var retrievedProduct = await _productGateway.GetProductById(product.ProductId);
        Assert.NotNull(retrievedProduct);
        Assert.Equal(createdProduct.ProductId, retrievedProduct.ProductId);

        // Atualizar produto
        var updatedProduct = new Product("Updated Name", "Updated Description", 299.99m, 200);
        updatedProduct.ProductId = product.ProductId;
        await _productGateway.UpdateProduct(updatedProduct);

        // Atualizar quantidade
        await _productGateway.UpdateQuantityProduct(250, product.ProductId);

        // Buscar novamente para verificar consistência
        var finalProduct = await _productGateway.GetProductById(product.ProductId);
        Assert.NotNull(finalProduct);
        Assert.Equal(product.ProductId, finalProduct.ProductId);

        // Assert
        _mockRepository.Verify(x => x.AddAsync(It.IsAny<ProductEntity>()), Times.Once);
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Exactly(2));
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<ProductEntity>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task TransactionSimulation_WithMultipleOperations_ShouldHandleCorrectly()
    {
        // Arrange
        var products = AutoFaker.Generate<Product>(3);
        var productEntities = products.Select(p => new ProductEntity
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            StockQuantity = p.StockQuantity,
            Reservation = p.Reservation
        }).ToList();

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<ProductEntity>()))
            .ReturnsAsync((ProductEntity entity) => entity);

        _mockRepository
            .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(productEntities);

        // Act - Simular transação com múltiplas operações
        var createdProducts = new List<Product>();
        
        // Criar produtos
        foreach (var product in products)
        {
            var created = await _productGateway.AddProduct(product);
            createdProducts.Add(created);
        }

        // Atualizar quantidades
        foreach (var product in createdProducts)
        {
            await _productGateway.UpdateQuantityProduct(product.StockQuantity + 50, product.ProductId);
        }

        // Verificar estado final
        var allProducts = await _productGateway.GetAllProducts();
        var allProductsList = allProducts.ToList();

        // Assert
        Assert.Equal(3, createdProducts.Count);
        Assert.Equal(3, allProductsList.Count);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<ProductEntity>()), Times.Exactly(3));
        _mockRepository.Verify(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()), Times.Exactly(3));
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task EdgeCases_WithBoundaryValues_ShouldHandleCorrectly()
    {
        // Arrange
        var product = new Product("Edge Case Product", "Edge Case Description", 0.01m, 1);
        var productEntity = new ProductEntity
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Reservation = 0
        };

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<ProductEntity>()))
            .ReturnsAsync(productEntity);

        _mockRepository
            .Setup(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(productEntity);

        // Act & Assert - Testar valores limites
        var createdProduct = await _productGateway.AddProduct(product);
        Assert.NotNull(createdProduct);
        Assert.Equal(0.01m, createdProduct.Price);
        Assert.Equal(1, createdProduct.StockQuantity);

        // Atualizar para quantidade zero
        await _productGateway.UpdateQuantityProduct(0, product.ProductId);

        // Atualizar para quantidade máxima
        await _productGateway.UpdateQuantityProduct(int.MaxValue, product.ProductId);

        // Buscar produto
        var retrievedProduct = await _productGateway.GetProductById(product.ProductId);
        Assert.NotNull(retrievedProduct);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<ProductEntity>()), Times.Once);
        _mockRepository.Verify(x => x.UpdateQuantityProduct(It.IsAny<int>(), It.IsAny<Guid>()), Times.Exactly(2));
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Once);
    }
}
