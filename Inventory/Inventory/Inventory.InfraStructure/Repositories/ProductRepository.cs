using Inventory.InfraStructure.Configure;
using Inventory.InfraStructure.Entities;
using Inventory.InfraStructure.Repositories.Abstractions;
using Inventory.InfraStructure.Tools;
using Microsoft.EntityFrameworkCore;

namespace Inventory.InfraStructure.Repositories;

public class ProductRepository(DataContext context) 
    : RepositoryBase<ProductEntity>(context), IProductRepository
{
    private readonly DataContext _context1 = context;

    public async Task<ProductEntity> GetProduct(ProductEntity product)
    {
        if (product?.ProductId == Guid.Empty)
            throw new ArgumentException("ProductId não pode ser vazio");

        try
        {
            var entity = await _context1.Set<ProductEntity>().AsNoTracking()
                .FirstAsync(x => x.ProductId == product.ProductId);
            return entity;
        }
        catch (InvalidOperationException)
        {
            throw new DataAccessException($"Produto com ID {product.ProductId} não encontrado");
        }
    }
    
    public async Task UpdateQuantityProduct(int newQuantity, Guid productId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId não pode ser vazio");

        if (newQuantity < 0)
            throw new ArgumentException("Quantidade não pode ser negativa");

        try
        {
            var entity = await _context1.Set<ProductEntity>()
                .FirstAsync(x => x.ProductId == productId);
            entity.StockQuantity = newQuantity;
            _context1.Set<ProductEntity>().Update(entity);
            await _context1.SaveChangesAsync();
        }
        catch (InvalidOperationException)
        {
            throw new DataAccessException($"Produto com ID {productId} não encontrado para atualização");
        }
    }
}
    