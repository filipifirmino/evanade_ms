using Sales.Application.Entities;
using Sales.Infrastructure.Entities;

namespace Sales.Infrastructure.Mappers;

public static class OrderMapper
{
    public static Order ToDomain(this OrderEntity orderEntity)
        => new Order
        {
            OrderId = orderEntity.OrderId,
            CustomerId = orderEntity.CustomerId,
            Items = orderEntity.Items.Select(item => item.ToDomain()).ToList(),
            TotalAmount =  orderEntity.TotalAmount,
            Status = orderEntity.Status.ToOrderStatus()
        };
    
    public static OrderEntity ToEntity(this Order order)
        => new OrderEntity
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            Items = order.Items.Select(x => x.ToEntity()).ToList(),
            TotalAmount =  order.TotalAmount,
            Status = order.Status.ToStatusEntity()
        };
}