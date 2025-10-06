using AutoBogus;
using Inventory.InfraStructure.Configure;
using Inventory.InfraStructure.Entities;
using Inventory.InfraStructure.Repositories;
using Inventory.InfraStructure.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Inventory.Tests.InfraStructureTests.RepositoryTests;

public class ProductRepositoryIntegrationTests : IDisposable
{
    private readonly DataContext _context;
    private readonly ProductRepository _repository;

    public ProductRepositoryIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockConfiguration = new Mock<IConfiguration>();
        _context = new DataContext(options, mockConfiguration.Object);
        _repository = new ProductRepository(_context);
    }

    [Fact]
    public async Task CompleteProductLifecycle_ShouldWorkCorrectly()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        productEntity.StockQuantity = Math.Max(0, productEntity.StockQuantity); // Ensure non-negative

        // Act & Assert - Ciclo completo de vida do produto
        // 1. Adicionar produto
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        // 2. Buscar produto
        var searchProduct = new ProductEntity { ProductId = productEntity.ProductId };
        var retrievedProduct = await _repository.GetProduct(searchProduct);
        Assert.NotNull(retrievedProduct);
        Assert.Equal(productEntity.ProductId, retrievedProduct.ProductId);

        // 3. Atualizar quantidade
        var newQuantity = productEntity.StockQuantity + 50;
        await _repository.UpdateQuantityProduct(newQuantity, productEntity.ProductId);

        // 4. Verificar atualização
        var updatedProduct = await _repository.GetProduct(searchProduct);
        Assert.NotNull(updatedProduct);
        Assert.Equal(newQuantity, updatedProduct.StockQuantity);

        // 5. Atualizar quantidade novamente
        var finalQuantity = newQuantity + 25;
        await _repository.UpdateQuantityProduct(finalQuantity, productEntity.ProductId);

        // 6. Verificar atualização final
        var finalProduct = await _repository.GetProduct(searchProduct);
        Assert.NotNull(finalProduct);
        Assert.Equal(finalQuantity, finalProduct.StockQuantity);
    }


    [Fact]
    public async Task ConcurrentOperations_ShouldHandleCorrectly()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        productEntity.StockQuantity = 1000;
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        var searchProduct = new ProductEntity { ProductId = productEntity.ProductId };

        // Act - Operações concorrentes
        var tasks = new List<Task>
        {
            _repository.GetProduct(searchProduct),
            _repository.UpdateQuantityProduct(1100, productEntity.ProductId),
            _repository.UpdateQuantityProduct(1200, productEntity.ProductId),
            _repository.UpdateQuantityProduct(1300, productEntity.ProductId)
        };

        await Task.WhenAll(tasks);

        // Assert
        var finalProduct = await _repository.GetProduct(searchProduct);
        Assert.NotNull(finalProduct);
        // A última atualização deve ser 1300
        Assert.Equal(1300, finalProduct.StockQuantity);
    }

    [Fact]
    public async Task ErrorRecovery_ShouldHandlePartialFailures()
    {
        // Arrange
        var productEntities = AutoFaker.Generate<ProductEntity>(3);
        foreach (var entity in productEntities)
        {
            entity.ProductId = Guid.NewGuid();
            entity.StockQuantity = 100; // Set fixed quantity to avoid randomness
            _context.Set<ProductEntity>().Add(entity);
        }
        await _context.SaveChangesAsync();

        // Act & Assert
        var successfulUpdates = 0;
        var failedUpdates = 0;

        foreach (var entity in productEntities)
        {
            try
            {
                if (entity.ProductId == productEntities[1].ProductId)
                {
                    // Simular falha no segundo produto
                    await _repository.UpdateQuantityProduct(-1, entity.ProductId);
                }
                else
                {
                    await _repository.UpdateQuantityProduct(150, entity.ProductId);
                    successfulUpdates++;
                }
            }
            catch (ArgumentException)
            {
                failedUpdates++;
            }
        }

        // Verificar que 2 produtos foram atualizados com sucesso e 1 falhou
        Assert.Equal(2, successfulUpdates);
        Assert.Equal(1, failedUpdates);

        // Verificar que os produtos atualizados com sucesso mantêm as novas quantidades
        for (int i = 0; i < productEntities.Count; i++)
        {
            if (i == 1) continue; // Pular o produto que falhou

            var searchProduct = new ProductEntity { ProductId = productEntities[i].ProductId };
            var retrievedProduct = await _repository.GetProduct(searchProduct);
            
            Assert.NotNull(retrievedProduct);
            Assert.Equal(150, retrievedProduct.StockQuantity);
        }
    }


    [Fact]
    public async Task DataConsistency_WithComplexOperations_ShouldMaintainIntegrity()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        productEntity.StockQuantity = 100;
        productEntity.Price = 99.99m;
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        var searchProduct = new ProductEntity { ProductId = productEntity.ProductId };

        // Act - Operações complexas que devem manter consistência
        var retrievedProduct = await _repository.GetProduct(searchProduct);
        Assert.NotNull(retrievedProduct);
        Assert.Equal(100, retrievedProduct.StockQuantity);

        // Atualizar quantidade
        await _repository.UpdateQuantityProduct(200, productEntity.ProductId);

        // Verificar se a quantidade foi atualizada
        var updatedProduct = await _repository.GetProduct(searchProduct);
        Assert.NotNull(updatedProduct);
        Assert.Equal(200, updatedProduct.StockQuantity);
        Assert.Equal(99.99m, updatedProduct.Price); // Preço deve permanecer inalterado

        // Atualizar quantidade novamente
        await _repository.UpdateQuantityProduct(300, productEntity.ProductId);

        // Verificar consistência final
        var finalProduct = await _repository.GetProduct(searchProduct);
        Assert.NotNull(finalProduct);
        Assert.Equal(300, finalProduct.StockQuantity);
        Assert.Equal(99.99m, finalProduct.Price);
        Assert.Equal(productEntity.Name, finalProduct.Name);
        Assert.Equal(productEntity.Description, finalProduct.Description);
    }

    [Fact]
    public async Task TransactionSimulation_WithMultipleOperations_ShouldHandleCorrectly()
    {
        // Arrange
        var productEntities = AutoFaker.Generate<ProductEntity>(5);
        foreach (var entity in productEntities)
        {
            entity.ProductId = Guid.NewGuid();
            entity.StockQuantity = 100;
            _context.Set<ProductEntity>().Add(entity);
        }
        await _context.SaveChangesAsync();

        // Act - Simular transação com múltiplas operações
        var updateTasks = productEntities.Select(entity => 
            _repository.UpdateQuantityProduct(entity.StockQuantity + 50, entity.ProductId));
        
        await Task.WhenAll(updateTasks);

        // Verificar estado final
        var verificationTasks = productEntities.Select(async entity =>
        {
            var searchProduct = new ProductEntity { ProductId = entity.ProductId };
            var retrievedProduct = await _repository.GetProduct(searchProduct);
            return retrievedProduct;
        });

        var finalProducts = await Task.WhenAll(verificationTasks);

        // Assert
        Assert.Equal(5, finalProducts.Length);
        foreach (var product in finalProducts)
        {
            Assert.NotNull(product);
            Assert.Equal(150, product.StockQuantity); // 100 + 50
        }
    }

    [Fact]
    public async Task EdgeCases_WithBoundaryValues_ShouldHandleCorrectly()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        productEntity.StockQuantity = 1;
        productEntity.Price = 0.01m;
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        var searchProduct = new ProductEntity { ProductId = productEntity.ProductId };

        // Act & Assert - Testar valores limites
        var retrievedProduct = await _repository.GetProduct(searchProduct);
        Assert.NotNull(retrievedProduct);
        Assert.Equal(1, retrievedProduct.StockQuantity);
        Assert.Equal(0.01m, retrievedProduct.Price);

        // Atualizar para quantidade zero
        await _repository.UpdateQuantityProduct(0, productEntity.ProductId);
        var zeroQuantityProduct = await _repository.GetProduct(searchProduct);
        Assert.NotNull(zeroQuantityProduct);
        Assert.Equal(0, zeroQuantityProduct.StockQuantity);

        // Atualizar para quantidade máxima
        await _repository.UpdateQuantityProduct(int.MaxValue, productEntity.ProductId);
        var maxQuantityProduct = await _repository.GetProduct(searchProduct);
        Assert.NotNull(maxQuantityProduct);
        Assert.Equal(int.MaxValue, maxQuantityProduct.StockQuantity);
    }

    [Fact]
    public async Task ComplexDataTypes_WithSpecialCharacters_ShouldHandleCorrectly()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        productEntity.Name = "Produto com Acentuação: ção, ñ, ü, é, á, í, ó, ú";
        productEntity.Description = "Descrição com símbolos especiais: @#$%^&*()_+-=[]{}|;':\",./<>?";
        productEntity.Price = 123.456789m;
        productEntity.StockQuantity = 999;
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        var searchProduct = new ProductEntity { ProductId = productEntity.ProductId };

        // Act
        var retrievedProduct = await _repository.GetProduct(searchProduct);

        // Assert
        Assert.NotNull(retrievedProduct);
        Assert.Equal(productEntity.Name, retrievedProduct.Name);
        Assert.Equal(productEntity.Description, retrievedProduct.Description);
        Assert.Equal(productEntity.Price, retrievedProduct.Price);
        Assert.Equal(productEntity.StockQuantity, retrievedProduct.StockQuantity);

        // Atualizar quantidade
        await _repository.UpdateQuantityProduct(1000, productEntity.ProductId);
        var updatedProduct = await _repository.GetProduct(searchProduct);
        
        Assert.NotNull(updatedProduct);
        Assert.Equal(1000, updatedProduct.StockQuantity);
        Assert.Equal(productEntity.Name, updatedProduct.Name); // Nome deve permanecer inalterado
        Assert.Equal(productEntity.Description, updatedProduct.Description); // Descrição deve permanecer inalterada
        Assert.Equal(productEntity.Price, updatedProduct.Price); // Preço deve permanecer inalterado
    }


    [Fact]
    public async Task StressTest_WithRapidOperations_ShouldHandleCorrectly()
    {
        // Arrange
        var productEntity = AutoFaker.Generate<ProductEntity>();
        productEntity.ProductId = Guid.NewGuid();
        productEntity.StockQuantity = 1000;
        
        _context.Set<ProductEntity>().Add(productEntity);
        await _context.SaveChangesAsync();

        var searchProduct = new ProductEntity { ProductId = productEntity.ProductId };

        // Act - Operações rápidas e repetitivas
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(_repository.UpdateQuantityProduct(1000 + i, productEntity.ProductId));
        }

        await Task.WhenAll(tasks);

        // Assert
        var finalProduct = await _repository.GetProduct(searchProduct);
        Assert.NotNull(finalProduct);
        Assert.Equal(1099, finalProduct.StockQuantity); // 1000 + 99 (última atualização)
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
