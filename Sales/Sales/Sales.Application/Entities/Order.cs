using Sales.Application.Enums;
using Sales.Application.ValueObject;

namespace Sales.Application.Entities;

public class Order
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public  List<OrderItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public  Status Status { get; set; }


    public void AddItem(Guid productId, int quantity, decimal price)
    {
        Items.Add(new OrderItem(productId, quantity, price));
    }

    public void CalculateTotal()
    {
        TotalAmount = Items.Sum(x => x.UnitPrice * x.Quantity);
    }
    public void Confirm()
    {
        Status = Status.Confirmed;
    }
    public void Cancel(string reason)
    {
        Status = Status.Cancelled;
        var reasonMessage = $"Order {OrderId} was cancelled. Reason: {reason}";
    }  
}

