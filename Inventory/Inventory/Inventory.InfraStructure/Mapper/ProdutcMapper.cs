using Inventory.Application.Entities;
using Inventory.InfraStructure.Entitys;

namespace Inventory.InfraStructure.Mapper;

public static class ProductMapper
{
    public static ProductEntity ToProductEntity(this Product product)
        => new ProductEntity
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity
        };

    public static Product ToProduct(this ProductEntity entity)
        => new Product
        {
            Name = entity.Name,
            Description = entity.Description,
            Price = entity.Price,
            StockQuantity = entity.StockQuantity,
            ProductId = entity.ProductId
        };
}