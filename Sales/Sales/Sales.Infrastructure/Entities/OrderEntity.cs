using Sales.Application.Enums;
using Sales.Application.ValueObject;

namespace Sales.Infrastructure.Entities;

public class OrderEntity
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    private List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public  Status Status { get; set; }
}