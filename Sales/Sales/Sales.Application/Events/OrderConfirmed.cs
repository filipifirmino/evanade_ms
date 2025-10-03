using Sales.Application.AbstractionRabbit;
using Sales.Application.Enums;

namespace Sales.Application.Events;

public class OrderConfirmed : IEventWithQueueConfiguration
{
    public string QueueName  => "inventory-stock-update-confirmed";
    
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantityReserved { get; set; }
    public int NewStockQuantity { get; set; }
    public DateTime ConfirmedAt { get; set; } = DateTime.UtcNow;
    public Status Status { get; set; }

    // Construtor padrão necessário para deserialização JSON
    public OrderConfirmed()
    {
    }

    public OrderConfirmed(Guid orderId, Guid productId, string productName, 
        int quantityReserved, int newStockQuantity, Status status)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        QuantityReserved = quantityReserved;
        NewStockQuantity = newStockQuantity;
        Status = status;
    }
}