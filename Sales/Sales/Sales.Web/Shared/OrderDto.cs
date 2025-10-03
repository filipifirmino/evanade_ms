using Sales.Application.Enums;
using Sales.Application.ValueObject;

namespace Sales.Web.Shared;

public abstract class OrderDto
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public Status Status { get; set; } = Status.Created;
}

