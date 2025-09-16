namespace Sales.Infrastructure.ValueObjects;

public class OrderItemEntity(Guid productId, int quantity, decimal unitPrice)
{
    public Guid ProductId { get; set; } = productId;
    public int Quantity { get; set; } = quantity;
    public decimal UnitPrice { get; set; } = unitPrice;
}