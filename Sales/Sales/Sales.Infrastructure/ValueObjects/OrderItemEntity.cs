using Sales.Application.ValueObject;

namespace Sales.Infrastructure.ValueObjects;

public class OrderItemEntity : OrderItem
{
    public Guid OrderId { get; set; }
    
    public OrderItemEntity(Guid productId, int quantity, decimal unitPrice) 
        : base(productId, quantity, unitPrice)
    {
    }
}