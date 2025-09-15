using Inventory.InfraStructure.Entities;

namespace Inventory.InfraStructure.Repositories.Abstractions;

public interface IProductRepository : IRepositoryBase<ProductEntity>
{
    Task<ProductEntity> GetProduct(ProductEntity product);
}