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
        // TODO: Implementar log ou notificação de cancelamento
    }
    
    public bool IsValid()
    {
        return TotalAmount > 0 && 
               Items.Count > 0 && 
               Items.All(item => item.Quantity > 0 && item.UnitPrice > 0);
    }
    
    public string GetValidationErrors()
    {
        var errors = new List<string>();
        
        if (TotalAmount <= 0)
            errors.Add("Total amount must be greater than zero");
            
        if (Items.Count == 0)
            errors.Add("Order must have at least one item");
            
        var invalidItems = Items.Where(item => item.Quantity <= 0 || item.UnitPrice <= 0).ToList();
        if (invalidItems.Any())
            errors.Add($"Invalid items found: {invalidItems.Count} items have invalid quantity or price");
            
        return string.Join("; ", errors);
    }
}

