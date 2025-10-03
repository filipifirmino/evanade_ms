using Sales.Application.Enums;
using Sales.Infrastructure.ValueObjects;

namespace Sales.Infrastructure.Entities;

public class OrderEntity
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public List<OrderItemEntity> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public Status Status { get; set; }
}