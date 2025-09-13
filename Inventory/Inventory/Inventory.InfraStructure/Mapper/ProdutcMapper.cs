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
        => new Product (entity.Name,entity.Description, entity.Price, entity.StockQuantity)
        {
        };
}