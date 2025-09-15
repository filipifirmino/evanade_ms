using Inventory.Application.Entities;
using Inventory.Web.RequestsDto;
using Inventory.Web.ResponsesDto;

namespace Inventory.Web.Mappers;

public static class ProductMapper
{
    public static Product ToProduct(this ProductRequest product)
        => new Product(product.Name, product.Description, product.Price, product.StockQuantity)
        {
            ProductId = product.ProductId
        };
    
    public static ProductResponse ToProductResponse(this Product product)
        => new ProductResponse
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity
        };

}