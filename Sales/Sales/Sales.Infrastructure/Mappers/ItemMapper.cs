using Sales.Application.ValueObject;
using Sales.Infrastructure.ValueObjects;

namespace Sales.Infrastructure.Mappers;

public static class ItemMapper
{
    public static OrderItem ToDomain(this OrderItemEntity entity)
    => new OrderItem(entity.ProductId, entity.Quantity, entity.UnitPrice);
    
    public static OrderItemEntity ToEntity(this OrderItem domain)
        => new OrderItemEntity(domain.ProductId, domain.Quantity, domain.UnitPrice);
}