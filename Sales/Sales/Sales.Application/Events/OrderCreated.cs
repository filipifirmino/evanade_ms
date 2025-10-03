using Sales.Application.AbstractionRabbit;
using Sales.Application.ValueObject;

namespace Sales.Application.Events;

public class OrderCreated : IEventWithQueueConfiguration
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = new();
    
    public string QueueName => "order-created-queue";

    public OrderCreated(Guid orderId, string customerId, decimal totalAmount, List<OrderItem> items)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Items = items;
    }
}