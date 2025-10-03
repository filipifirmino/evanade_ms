namespace APIGateway.ApplicationCore.DTOs;

public class OrderRequest
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public Status Status { get; set; } = Status.Created;
}

public class OrderItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public enum Status
{
    Created,
    Confirmed,
    Cancelled,
    Failed
}