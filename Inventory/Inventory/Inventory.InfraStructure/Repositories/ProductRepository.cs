using Inventory.InfraStructure.Configure;
using Inventory.InfraStructure.Entities;
using Inventory.InfraStructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Inventory.InfraStructure.Repositories;

public class ProductRepository(DataContext context) 
    : RepositoreBase<ProductEntity>(context), IProductRepository
{
    private readonly DataContext _context1 = context;

    public async Task<ProductEntity> GetProduct(ProductEntity product)
    {
        var entity = await _context1.Set<ProductEntity>().AsNoTracking()
            .FirstAsync(x => x.ProductId == product.ProductId);
        return entity;
    }
    
    public async Task UpdateQuantityProduct(int newQuantity, Guid productId)
    {
        var entity = await _context1.Set<ProductEntity>()
            .FirstAsync(x => x.ProductId == productId);
        entity.StockQuantity = newQuantity;
        _context1.Set<ProductEntity>().Update(entity);
        await _context1.SaveChangesAsync();
    }
}
    