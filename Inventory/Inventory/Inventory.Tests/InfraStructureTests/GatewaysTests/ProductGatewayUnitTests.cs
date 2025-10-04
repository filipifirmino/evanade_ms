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

public class ProductGatewayUnitTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<ILogger<ProductGateway>> _mockLogger;
    private readonly ProductGateway _productGateway;

    public ProductGatewayUnitTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _mockLogger = new Mock<ILogger<ProductGateway>>();
        _productGateway = new ProductGateway(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task AddProduct_WithValidProduct_ShouldReturnCreatedProduct()
    {
        // Arrange
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

        // Act
        var result = await _productGateway.AddProduct(product);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(product.ProductId, result.ProductId);
        Assert.Equal(product.Name, result.Name);
        Assert.Equal(product.Description, result.Description);
        Assert.Equal(product.Price, result.Price);
        Assert.Equal(product.StockQuantity, result.StockQuantity);

        _mockRepository.Verify(x => x.AddAsync(It.IsAny<ProductEntity>()), Times.Once);
    }


    [Fact]
    public async Task UpdateProduct_WithValidProduct_ShouldUpdateSuccessfully()
    {
        // Arrange
        var product = AutoFaker.Generate<Product>();

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ProductEntity>()))
            .Returns(Task.CompletedTask);

        // Act
        await _productGateway.UpdateProduct(product);

        // Assert
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<ProductEntity>()), Times.Once);
        VerifyLogInformation("Update product");
    }


    [Fact]
    public async Task UpdateQuantityProduct_WithValidParameters_ShouldUpdateSuccessfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var newQuantity = 50;

        _mockRepository
            .Setup(x => x.UpdateQuantityProduct(newQuantity, productId))
            .Returns(Task.CompletedTask);

        // Act
        await _productGateway.UpdateQuantityProduct(newQuantity, productId);

        // Assert
        _mockRepository.Verify(x => x.UpdateQuantityProduct(newQuantity, productId), Times.Once);
        VerifyLogInformation($"Update quantity product: {productId} to new quantity: {newQuantity}");
    }


    [Fact]
    public async Task DeleteProduct_WithValidProduct_ShouldDeleteSuccessfully()
    {
        // Arrange
        var product = AutoFaker.Generate<Product>();

        _mockRepository
            .Setup(x => x.DeleteAsync(It.IsAny<ProductEntity>()))
            .Returns(Task.CompletedTask);

        // Act
        await _productGateway.DeleteProduct(product);

        // Assert
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<ProductEntity>()), Times.Once);
        VerifyLogInformation("Delelete produtc:");
    }


    [Fact]
    public async Task GetProductById_WithExistingProduct_ShouldReturnProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = productId;

        _mockRepository
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync(productEntity);

        // Act
        var result = await _productGateway.GetProductById(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productEntity.ProductId, result.ProductId);
        Assert.Equal(productEntity.Name, result.Name);
        Assert.Equal(productEntity.Description, result.Description);
        Assert.Equal(productEntity.Price, result.Price);
        Assert.Equal(productEntity.StockQuantity, result.StockQuantity);

        _mockRepository.Verify(x => x.GetByIdAsync(productId), Times.Once);
    }

    [Fact]
    public async Task GetProductById_WithNonExistingProduct_ShouldReturnNull()
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetByIdAsync(productId))
            .ReturnsAsync((ProductEntity?)null);

        // Act
        var result = await _productGateway.GetProductById(productId);

        // Assert
        Assert.Null(result);
        _mockRepository.Verify(x => x.GetByIdAsync(productId), Times.Once);
    }


    [Fact]
    public async Task GetAllProducts_WithExistingProducts_ShouldReturnAllProducts()
    {
        // Arrange
        var productEntities = AutoFaker.Generate<ProductEntity>(3);

        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(productEntities);

        // Act
        var result = await _productGateway.GetAllProducts();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());

        var resultList = result.ToList();
        for (int i = 0; i < productEntities.Count; i++)
        {
            Assert.Equal(productEntities[i].ProductId, resultList[i].ProductId);
            Assert.Equal(productEntities[i].Name, resultList[i].Name);
            Assert.Equal(productEntities[i].Description, resultList[i].Description);
            Assert.Equal(productEntities[i].Price, resultList[i].Price);
            Assert.Equal(productEntities[i].StockQuantity, resultList[i].StockQuantity);
        }

        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllProducts_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyList = new List<ProductEntity>();

        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(emptyList);

        // Act
        var result = await _productGateway.GetAllProducts();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }


    [Fact]
    public async Task AddProduct_ShouldMapProductCorrectly()
    {
        // Arrange
        var product = new Product("Test Product", "Test Description", 99.99m, 100);
        var capturedEntity = new ProductEntity();
        
        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<ProductEntity>()))
            .Callback<ProductEntity>(entity => capturedEntity = entity)
            .ReturnsAsync(capturedEntity);

        // Act
        await _productGateway.AddProduct(product);

        // Assert
        Assert.Equal(product.ProductId, capturedEntity.ProductId);
        Assert.Equal(product.Name, capturedEntity.Name);
        Assert.Equal(product.Description, capturedEntity.Description);
        Assert.Equal(product.Price, capturedEntity.Price);
        Assert.Equal(product.StockQuantity, capturedEntity.StockQuantity);
        Assert.Equal(product.Reservation, capturedEntity.Reservation);
    }

    [Fact]
    public async Task UpdateProduct_ShouldMapProductCorrectly()
    {
        // Arrange
        var product = new Product("Updated Product", "Updated Description", 149.99m, 75);
        var capturedEntity = new ProductEntity();
        
        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<ProductEntity>()))
            .Callback<ProductEntity>(entity => capturedEntity = entity)
            .Returns(Task.CompletedTask);

        // Act
        await _productGateway.UpdateProduct(product);

        // Assert
        Assert.Equal(product.ProductId, capturedEntity.ProductId);
        Assert.Equal(product.Name, capturedEntity.Name);
        Assert.Equal(product.Description, capturedEntity.Description);
        Assert.Equal(product.Price, capturedEntity.Price);
        Assert.Equal(product.StockQuantity, capturedEntity.StockQuantity);
        Assert.Equal(product.Reservation, capturedEntity.Reservation);
    }

    [Fact]
    public async Task DeleteProduct_ShouldMapProductCorrectly()
    {
        // Arrange
        var product = new Product("Delete Product", "Delete Description", 199.99m, 25);
        var capturedEntity = new ProductEntity();
        
        _mockRepository
            .Setup(x => x.DeleteAsync(It.IsAny<ProductEntity>()))
            .Callback<ProductEntity>(entity => capturedEntity = entity)
            .Returns(Task.CompletedTask);

        // Act
        await _productGateway.DeleteProduct(product);

        // Assert
        Assert.Equal(product.ProductId, capturedEntity.ProductId);
        Assert.Equal(product.Name, capturedEntity.Name);
        Assert.Equal(product.Description, capturedEntity.Description);
        Assert.Equal(product.Price, capturedEntity.Price);
        Assert.Equal(product.StockQuantity, capturedEntity.StockQuantity);
        Assert.Equal(product.Reservation, capturedEntity.Reservation);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task UpdateQuantityProduct_WithDifferentQuantities_ShouldUpdateCorrectly(int quantity)
    {
        // Arrange
        var productId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.UpdateQuantityProduct(quantity, productId))
            .Returns(Task.CompletedTask);

        // Act
        await _productGateway.UpdateQuantityProduct(quantity, productId);

        // Assert
        _mockRepository.Verify(x => x.UpdateQuantityProduct(quantity, productId), Times.Once);
    }

    [Fact]
    public async Task GetAllProducts_WithLargeDataset_ShouldHandleCorrectly()
    {
        // Arrange
        var productEntities = AutoFaker.Generate<ProductEntity>(1000);

        _mockRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(productEntities);

        // Act
        var result = await _productGateway.GetAllProducts();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000, result.Count());
        _mockRepository.Verify(x => x.GetAllAsync(), Times.Once);
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
}
