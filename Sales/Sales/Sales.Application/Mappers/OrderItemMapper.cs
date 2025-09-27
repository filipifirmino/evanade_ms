using Sales.Application.Events;
using Sales.Application.ValueObject;

namespace Sales.Application.Mappers;

public static class OrderItemMapper
{
    
    public static OrderItemEvent ToEvent(this OrderItem domain)
        => new OrderItemEvent
        {
            ProductId = domain.ProductId,
            Quantity = domain.Quantity,
            UnitPrice = domain.UnitPrice
        };
}