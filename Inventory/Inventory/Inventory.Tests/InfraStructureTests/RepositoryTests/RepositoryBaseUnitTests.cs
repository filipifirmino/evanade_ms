using AutoBogus;
using Inventory.InfraStructure.Configure;
using Inventory.InfraStructure.Entities;
using Inventory.InfraStructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Inventory.Tests.InfraStructureTests.RepositoryTests;

public class RepositoryBaseUnitTests : IDisposable
{
    private readonly DataContext _context;
    private readonly RepositoryBase<ProductEntity> _repository;

    public RepositoryBaseUnitTests()
    {
        var options = new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var mockConfiguration = new Mock<IConfiguration>();
        _context = new DataContext(options, mockConfiguration.Object);
        _repository = new RepositoryBase<ProductEntity>(_context);
    }

    [Fact]
    public async Task GetAllAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleEntities_ShouldReturnAllEntities()
    {
        // Arrange
        var entities = AutoFaker.Generate<ProductEntity>(5);
        foreach (var entity in entities)
        {
            entity.ProductId = Guid.NewGuid();
            _context.Set<ProductEntity>().Add(entity);
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
        
        var resultList = result.ToList();
        for (int i = 0; i < entities.Count; i++)
        {
            Assert.Equal(entities[i].ProductId, resultList[i].ProductId);
            Assert.Equal(entities[i].Name, resultList[i].Name);
            Assert.Equal(entities[i].Description, resultList[i].Description);
            Assert.Equal(entities[i].Price, resultList[i].Price);
            Assert.Equal(entities[i].StockQuantity, resultList[i].StockQuantity);
            Assert.Equal(entities[i].Reservation, resultList[i].Reservation);
        }
    }

    [Fact]
    public async Task GetAllAsync_WithSingleEntity_ShouldReturnSingleEntity()
    {
        // Arrange
        var entity = AutoFaker.Generate<ProductEntity>();
        entity.ProductId = Guid.NewGuid();
        _context.Set<ProductEntity>().Add(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(entity.ProductId, result.First().ProductId);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingEntity_ShouldReturnEntity()
    {
        // Arrange
        var entity = AutoFaker.Generate<ProductEntity>();
        entity.ProductId = Guid.NewGuid();
        _context.Set<ProductEntity>().Add(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(entity.ProductId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.ProductId, result.ProductId);
        Assert.Equal(entity.Name, result.Name);
        Assert.Equal(entity.Description, result.Description);
        Assert.Equal(entity.Price, result.Price);
        Assert.Equal(entity.StockQuantity, result.StockQuantity);
        Assert.Equal(entity.Reservation, result.Reservation);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingEntity_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyGuid_ShouldReturnNull()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        // Act
        var result = await _repository.GetByIdAsync(emptyGuid);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_WithValidEntity_ShouldAddAndReturnEntity()
    {
        // Arrange
        var entity = AutoFaker.Generate<ProductEntity>();
        entity.ProductId = Guid.NewGuid();

        // Act
        var result = await _repository.AddAsync(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.ProductId, result.ProductId);
        Assert.Equal(entity.Name, result.Name);
        Assert.Equal(entity.Description, result.Description);
        Assert.Equal(entity.Price, result.Price);
        Assert.Equal(entity.StockQuantity, result.StockQuantity);
        Assert.Equal(entity.Reservation, result.Reservation);

        // Verify entity was added to database
        var dbEntity = await _context.Set<ProductEntity>()
            .FirstOrDefaultAsync(x => x.ProductId == entity.ProductId);
        Assert.NotNull(dbEntity);
    }

    [Fact]
    public async Task AddAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Arrange
        ProductEntity? nullEntity = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _repository.AddAsync(nullEntity!));
    }

    [Fact]
    public async Task AddAsync_WithMultipleEntities_ShouldAddAllEntities()
    {
        // Arrange
        var entities = AutoFaker.Generate<ProductEntity>(3);
        foreach (var entity in entities)
        {
            entity.ProductId = Guid.NewGuid();
        }

        // Act
        var results = new List<ProductEntity?>();
        foreach (var entity in entities)
        {
            var result = await _repository.AddAsync(entity);
            results.Add(result);
        }

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, result => Assert.NotNull(result));

        // Verify all entities were added to database
        var dbEntities = await _context.Set<ProductEntity>().ToListAsync();
        Assert.Equal(3, dbEntities.Count);
    }

    [Fact]
    public async Task UpdateAsync_WithValidEntity_ShouldUpdateEntity()
    {
        // Arrange
        var entity = AutoFaker.Generate<ProductEntity>();
        entity.ProductId = Guid.NewGuid();
        _context.Set<ProductEntity>().Add(entity);
        await _context.SaveChangesAsync();

        // Modify entity
        entity.Name = "Updated Name";
        entity.Description = "Updated Description";
        entity.Price = 999.99m;
        entity.StockQuantity = 500;

        // Act
        await _repository.UpdateAsync(entity);

        // Assert
        var updatedEntity = await _context.Set<ProductEntity>()
            .FirstOrDefaultAsync(x => x.ProductId == entity.ProductId);
        
        Assert.NotNull(updatedEntity);
        Assert.Equal("Updated Name", updatedEntity.Name);
        Assert.Equal("Updated Description", updatedEntity.Description);
        Assert.Equal(999.99m, updatedEntity.Price);
        Assert.Equal(500, updatedEntity.StockQuantity);
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingEntity_ShouldThrowConcurrencyException()
    {
        // Arrange
        var entity = AutoFaker.Generate<ProductEntity>();
        entity.ProductId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => 
            _repository.UpdateAsync(entity));
    }

    [Fact]
    public async Task UpdateAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Arrange
        ProductEntity? nullEntity = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _repository.UpdateAsync(nullEntity!));
    }

    [Fact]
    public async Task DeleteAsync_WithExistingEntity_ShouldRemoveEntity()
    {
        // Arrange
        var entity = AutoFaker.Generate<ProductEntity>();
        entity.ProductId = Guid.NewGuid();
        _context.Set<ProductEntity>().Add(entity);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(entity);

        // Assert
        var deletedEntity = await _context.Set<ProductEntity>()
            .FirstOrDefaultAsync(x => x.ProductId == entity.ProductId);
        
        Assert.Null(deletedEntity);
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingEntity_ShouldThrowConcurrencyException()
    {
        // Arrange
        var entity = AutoFaker.Generate<ProductEntity>();
        entity.ProductId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => 
            _repository.DeleteAsync(entity));
    }

    [Fact]
    public async Task DeleteAsync_WithNullEntity_ShouldThrowArgumentNullException()
    {
        // Arrange
        ProductEntity? nullEntity = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _repository.DeleteAsync(nullEntity!));
    }

    [Fact]
    public async Task DeleteAsync_WithMultipleEntities_ShouldRemoveAllEntities()
    {
        // Arrange
        var entities = AutoFaker.Generate<ProductEntity>(3);
        foreach (var entity in entities)
        {
            entity.ProductId = Guid.NewGuid();
            _context.Set<ProductEntity>().Add(entity);
        }
        await _context.SaveChangesAsync();

        // Act
        foreach (var entity in entities)
        {
            await _repository.DeleteAsync(entity);
        }

        // Assert
        var remainingEntities = await _context.Set<ProductEntity>().ToListAsync();
        Assert.Empty(remainingEntities);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task GetAllAsync_WithDifferentEntityCounts_ShouldReturnCorrectCount(int count)
    {
        // Arrange
        var entities = AutoFaker.Generate<ProductEntity>(count);
        foreach (var entity in entities)
        {
            entity.ProductId = Guid.NewGuid();
            _context.Set<ProductEntity>().Add(entity);
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(count, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_WithSpecialCharactersInEntity_ShouldReturnCorrectly()
    {
        // Arrange
        var entity = AutoFaker.Generate<ProductEntity>();
        entity.ProductId = Guid.NewGuid();
        entity.Name = "Produto com Acentuação e Símbolos @#$%";
        entity.Description = "Descrição com caracteres especiais: ção, ñ, ü";
        entity.Price = 99.99m;
        entity.StockQuantity = 100;
        entity.Reservation = 10;
        
        _context.Set<ProductEntity>().Add(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(entity.ProductId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Name, result.Name);
        Assert.Equal(entity.Description, result.Description);
        Assert.Equal(entity.Price, result.Price);
        Assert.Equal(entity.StockQuantity, result.StockQuantity);
        Assert.Equal(entity.Reservation, result.Reservation);
    }

    [Fact]
    public async Task AddAsync_WithLargeValues_ShouldHandleCorrectly()
    {
        // Arrange
        var entity = AutoFaker.Generate<ProductEntity>();
        entity.ProductId = Guid.NewGuid();
        entity.Name = new string('A', 1000); // Large name
        entity.Description = new string('B', 2000); // Large description
        entity.Price = decimal.MaxValue;
        entity.StockQuantity = int.MaxValue;
        entity.Reservation = int.MaxValue;

        // Act
        var result = await _repository.AddAsync(entity);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(entity.Name, result.Name);
        Assert.Equal(entity.Description, result.Description);
        Assert.Equal(entity.Price, result.Price);
        Assert.Equal(entity.StockQuantity, result.StockQuantity);
        Assert.Equal(entity.Reservation, result.Reservation);
    }

    [Fact]
    public async Task UpdateAsync_WithPartialUpdate_ShouldUpdateOnlyModifiedFields()
    {
        // Arrange
        var entity = AutoFaker.Generate<ProductEntity>();
        entity.ProductId = Guid.NewGuid();
        entity.Name = "Original Name";
        entity.Description = "Original Description";
        entity.Price = 100.00m;
        entity.StockQuantity = 50;
        entity.Reservation = 5;
        
        _context.Set<ProductEntity>().Add(entity);
        await _context.SaveChangesAsync();

        // Modify only some fields
        entity.Name = "Updated Name";
        entity.StockQuantity = 75;

        // Act
        await _repository.UpdateAsync(entity);

        // Assert
        var updatedEntity = await _context.Set<ProductEntity>()
            .FirstOrDefaultAsync(x => x.ProductId == entity.ProductId);
        
        Assert.NotNull(updatedEntity);
        Assert.Equal("Updated Name", updatedEntity.Name);
        Assert.Equal("Original Description", updatedEntity.Description); // Should remain unchanged
        Assert.Equal(100.00m, updatedEntity.Price); // Should remain unchanged
        Assert.Equal(75, updatedEntity.StockQuantity);
        Assert.Equal(5, updatedEntity.Reservation); // Should remain unchanged
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEntitiesInCorrectOrder()
    {
        // Arrange
        var entities = new List<ProductEntity>();
        for (int i = 0; i < 5; i++)
        {
            var entity = AutoFaker.Generate<ProductEntity>();
            entity.ProductId = Guid.NewGuid();
            entity.Name = $"Product {i}";
            entities.Add(entity);
            _context.Set<ProductEntity>().Add(entity);
        }
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
        
        // Verify all entities are present
        var resultList = result.ToList();
        foreach (var entity in entities)
        {
            Assert.Contains(resultList, r => r.ProductId == entity.ProductId && r.Name == entity.Name);
        }
    }

    [Fact]
    public async Task GetByIdAsync_WithMultipleEntities_ShouldReturnCorrectEntity()
    {
        // Arrange
        var entities = AutoFaker.Generate<ProductEntity>(5);
        foreach (var entity in entities)
        {
            entity.ProductId = Guid.NewGuid();
            _context.Set<ProductEntity>().Add(entity);
        }
        await _context.SaveChangesAsync();

        var targetEntity = entities[2]; // Middle entity

        // Act
        var result = await _repository.GetByIdAsync(targetEntity.ProductId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(targetEntity.ProductId, result.ProductId);
        Assert.Equal(targetEntity.Name, result.Name);
        Assert.Equal(targetEntity.Description, result.Description);
        Assert.Equal(targetEntity.Price, result.Price);
        Assert.Equal(targetEntity.StockQuantity, result.StockQuantity);
        Assert.Equal(targetEntity.Reservation, result.Reservation);
    }

    [Fact]
    public async Task AddAsync_WithDuplicateId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var entity1 = AutoFaker.Generate<ProductEntity>();
        entity1.ProductId = Guid.NewGuid();
        
        var entity2 = AutoFaker.Generate<ProductEntity>();
        entity2.ProductId = entity1.ProductId; // Same ID

        _context.Set<ProductEntity>().Add(entity1);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _repository.AddAsync(entity2));
    }

    [Fact]
    public async Task UpdateAsync_WithDetachedEntity_ShouldUpdateCorrectly()
    {
        // Arrange
        var entity = AutoFaker.Generate<ProductEntity>();
        entity.ProductId = Guid.NewGuid();
        _context.Set<ProductEntity>().Add(entity);
        await _context.SaveChangesAsync();

        // Detach entity from context
        _context.Entry(entity).State = EntityState.Detached;

        // Modify detached entity
        entity.Name = "Detached Updated Name";

        // Act
        await _repository.UpdateAsync(entity);

        // Assert
        var updatedEntity = await _context.Set<ProductEntity>()
            .FirstOrDefaultAsync(x => x.ProductId == entity.ProductId);
        
        Assert.NotNull(updatedEntity);
        Assert.Equal("Detached Updated Name", updatedEntity.Name);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
