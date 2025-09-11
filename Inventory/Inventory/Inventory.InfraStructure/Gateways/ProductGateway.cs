using Inventory.Application.AbstractionsGateways;
using Inventory.Application.Entities;

namespace Inventory.InfraStructure.Gateways;

public class ProductGateway : IProductGateway
{
    public ProductGateway()
    {
        
    }
    public Task AddProduct(Product product)
    {
        throw new NotImplementedException();
    }

    public Task UpdateProduct(Product product)
    {
        throw new NotImplementedException();
    }

    public Task DeleteProduct(Guid productId)
    {
        throw new NotImplementedException();
    }

    public Task<Product?> GetProductById(Guid productId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Product>> GetAllProducts()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Product>> GetProductsByCategory(string category)
    {
        throw new NotImplementedException();
    }
}