using Sales.Application.AbstractionRabbit;

namespace Sales.Application.Events;

public class OrderCreated : IEventWithQueueConfiguration
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<OrderItemEvent> Items { get; set; } = new();
    
    public string QueueName => "order-created-queue";

    public OrderCreated(Guid orderId, string customerId, decimal totalAmount, List<OrderItemEvent> items)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Items = items;
    }
}

public class OrderItemEvent
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}