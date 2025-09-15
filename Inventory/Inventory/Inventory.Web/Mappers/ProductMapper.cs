using Inventory.Application.Entities;
using Inventory.Web.RequestsDto;

namespace Inventory.Application.Mappers;

public static class ProductMapper
{
    public static Product ToProduct(this ProductRequest product)
        => new Product( product.Name, product.Description, product.Price, product.StockQuantity);

}