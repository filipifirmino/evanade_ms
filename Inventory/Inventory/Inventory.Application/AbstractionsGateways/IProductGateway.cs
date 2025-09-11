using Inventory.Application.Entities;

namespace Inventory.Application.AbstractionsGateways;

public interface IProductGateway
{
    Task AddProduct(Product product);
    Task UpdateProduct(Product product);
    Task DeleteProduct(Guid productId);
    Task<Product?> GetProductById(Guid productId);
    Task<IEnumerable<Product>> GetAllProducts();
    Task<IEnumerable<Product>> GetProductsByCategory(string category);
}