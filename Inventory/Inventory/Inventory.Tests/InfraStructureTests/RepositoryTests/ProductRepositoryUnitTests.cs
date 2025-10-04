using AutoBogus;
using Inventory.InfraStructure.Configure;
using Inventory.InfraStructure.Entities;
using Inventory.InfraStructure.Repositories;
using Inventory.InfraStructure.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Inventory.Tests.InfraStructureTests.RepositoryTests;

public class ProductRepositoryUnitTests : IDisposable
{
    private readonly DataContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryUnitTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockConfiguration = new Mock<IConfiguration>();
        _context = new DataContext(options, mockConfiguration.Object);
        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task GetProduct_WithValidProductId_ShouldReturnProduct()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        var searchProduct = new ProductEntity { ProductId = productEntity.ProductId };

        // Act
        var result = await _repository.GetProduct(searchProduct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productEntity.ProductId, result.ProductId);
        Assert.Equal(productEntity.Name, result.Name);
        Assert.Equal(productEntity.Description, result.Description);
        Assert.Equal(productEntity.Price, result.Price);
        Assert.Equal(productEntity.StockQuantity, result.StockQuantity);
        Assert.Equal(productEntity.Reservation, result.Reservation);
    }

    [Fact]
    public async Task GetProduct_WithNonExistentProductId_ShouldThrowDataAccessException()
    {
        // Arrange
        var nonExistentProductId = Guid.NewGuid();
        var searchProduct = new ProductEntity { ProductId = nonExistentProductId };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DataAccessException>(() => _repository.GetProduct(searchProduct));
        
        Assert.Contains($"Produto com ID {nonExistentProductId} não encontrado", exception.Message);
    }

    [Fact]
    public async Task GetProduct_WithEmptyProductId_ShouldThrowArgumentException()
    {
        // Arrange
        var searchProduct = new ProductEntity { ProductId = Guid.Empty };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _repository.GetProduct(searchProduct));
        
        Assert.Equal("ProductId não pode ser vazio", exception.Message);
    }


    [Fact]
    public async Task UpdateQuantityProduct_WithValidParameters_ShouldUpdateSuccessfully()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        productEntity.StockQuantity = 100;
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        var newQuantity = 150;

        // Act
        await _repository.UpdateQuantityProduct(newQuantity, productEntity.ProductId);

        // Assert
        var updatedEntity = await _context.Set<ProductEntity>()
            .FirstAsync(x => x.ProductId == productEntity.ProductId);
        
        Assert.Equal(newQuantity, updatedEntity.StockQuantity);
    }

    [Fact]
    public async Task UpdateQuantityProduct_WithNonExistentProductId_ShouldThrowDataAccessException()
    {
        // Arrange
        var nonExistentProductId = Guid.NewGuid();
        var newQuantity = 100;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DataAccessException>(() => 
            _repository.UpdateQuantityProduct(newQuantity, nonExistentProductId));
        
        Assert.Contains($"Produto com ID {nonExistentProductId} não encontrado para atualização", exception.Message);
    }

    [Fact]
    public async Task UpdateQuantityProduct_WithEmptyProductId_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyProductId = Guid.Empty;
        var newQuantity = 100;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _repository.UpdateQuantityProduct(newQuantity, emptyProductId));
        
        Assert.Equal("ProductId não pode ser vazio", exception.Message);
    }

    [Fact]
    public async Task UpdateQuantityProduct_WithNegativeQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        var negativeQuantity = -10;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _repository.UpdateQuantityProduct(negativeQuantity, productEntity.ProductId));
        
        Assert.Equal("Quantidade não pode ser negativa", exception.Message);
    }

    [Fact]
    public async Task UpdateQuantityProduct_WithZeroQuantity_ShouldUpdateSuccessfully()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        productEntity.StockQuantity = 100;
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        var zeroQuantity = 0;

        // Act
        await _repository.UpdateQuantityProduct(zeroQuantity, productEntity.ProductId);

        // Assert
        var updatedEntity = await _context.Set<ProductEntity>()
            .FirstAsync(x => x.ProductId == productEntity.ProductId);
        
        Assert.Equal(zeroQuantity, updatedEntity.StockQuantity);
    }

    [Fact]
    public async Task UpdateQuantityProduct_WithLargeQuantity_ShouldUpdateSuccessfully()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        productEntity.StockQuantity = 100;
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        var largeQuantity = int.MaxValue;

        // Act
        await _repository.UpdateQuantityProduct(largeQuantity, productEntity.ProductId);

        // Assert
        var updatedEntity = await _context.Set<ProductEntity>()
            .FirstAsync(x => x.ProductId == productEntity.ProductId);
        
        Assert.Equal(largeQuantity, updatedEntity.StockQuantity);
    }

    [Fact]
    public async Task GetProduct_ShouldUseAsNoTracking()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        var searchProduct = new ProductEntity { ProductId = productEntity.ProductId };

        // Act
        var result = await _repository.GetProduct(searchProduct);

        // Assert
        Assert.NotNull(result);
        
        // Verificar que o resultado não está sendo rastreado pelo contexto
        var entry = _context.Entry(result);
        Assert.Equal(EntityState.Detached, entry.State);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(int.MaxValue)]
    public async Task UpdateQuantityProduct_WithDifferentQuantities_ShouldUpdateCorrectly(int quantity)
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        productEntity.StockQuantity = 50;
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateQuantityProduct(quantity, productEntity.ProductId);

        // Assert
        var updatedEntity = await _context.Set<ProductEntity>()
            .FirstAsync(x => x.ProductId == productEntity.ProductId);
        
        Assert.Equal(quantity, updatedEntity.StockQuantity);
    }

    [Fact]
    public async Task GetProduct_WithMultipleProducts_ShouldReturnCorrectProduct()
    {
        // Arrange
        var productEntities = AutoFaker.Generate<ProductEntity>(5);
        foreach (var entity in productEntities)
        {
            entity.ProductId = Guid.NewGuid();
            _context.Set<ProductEntity>().Add(entity);
        }
        await _context.SaveChangesAsync();

        var targetProduct = productEntities[2]; // Produto do meio
        var searchProduct = new ProductEntity { ProductId = targetProduct.ProductId };

        // Act
        var result = await _repository.GetProduct(searchProduct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(targetProduct.ProductId, result.ProductId);
        Assert.Equal(targetProduct.Name, result.Name);
        Assert.Equal(targetProduct.Description, result.Description);
        Assert.Equal(targetProduct.Price, result.Price);
        Assert.Equal(targetProduct.StockQuantity, result.StockQuantity);
        Assert.Equal(targetProduct.Reservation, result.Reservation);
    }

    [Fact]
    public async Task UpdateQuantityProduct_WithMultipleUpdates_ShouldMaintainConsistency()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        productEntity.StockQuantity = 100;
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        // Act - Múltiplas atualizações
        await _repository.UpdateQuantityProduct(150, productEntity.ProductId);
        await _repository.UpdateQuantityProduct(200, productEntity.ProductId);
        await _repository.UpdateQuantityProduct(250, productEntity.ProductId);

        // Assert
        var updatedEntity = await _context.Set<ProductEntity>()
            .FirstAsync(x => x.ProductId == productEntity.ProductId);
        
        Assert.Equal(250, updatedEntity.StockQuantity);
    }

    [Fact]
    public async Task GetProduct_WithSpecialCharactersInName_ShouldReturnCorrectly()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        productEntity.Name = "Produto com Acentuação e Símbolos @#$%";
        productEntity.Description = "Descrição com caracteres especiais: ção, ñ, ü";
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        var searchProduct = new ProductEntity { ProductId = productEntity.ProductId };

        // Act
        var result = await _repository.GetProduct(searchProduct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productEntity.Name, result.Name);
        Assert.Equal(productEntity.Description, result.Description);
    }

    [Fact]
    public async Task UpdateQuantityProduct_WithDecimalPrice_ShouldMaintainPriceIntegrity()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        productEntity.Price = 99.99m;
        productEntity.StockQuantity = 100;
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        var newQuantity = 200;

        // Act
        await _repository.UpdateQuantityProduct(newQuantity, productEntity.ProductId);

        // Assert
        var updatedEntity = await _context.Set<ProductEntity>()
            .FirstAsync(x => x.ProductId == productEntity.ProductId);
        
        Assert.Equal(newQuantity, updatedEntity.StockQuantity);
        Assert.Equal(99.99m, updatedEntity.Price); // Preço deve permanecer inalterado
    }


    public void Dispose()
    {
        _context.Dispose();
    }
}
