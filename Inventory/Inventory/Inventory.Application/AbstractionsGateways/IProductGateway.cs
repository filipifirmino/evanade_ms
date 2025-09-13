using Inventory.Application.Entities;

namespace Inventory.Application.AbstractionsGateways;

public interface IProductGateway
{
    Task<Product> AddProduct(Product product);
    Task UpdateProduct(Product product);
    Task DeleteProduct(Product product);
    Task<Product?> GetProductById(Guid productId);
    Task<IEnumerable<Product>> GetAllProducts();
}