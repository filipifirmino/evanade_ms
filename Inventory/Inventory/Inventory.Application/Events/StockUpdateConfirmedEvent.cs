namespace Inventory.Application.Events;

public class StockUpdateConfirmedEvent
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantityReserved { get; set; }
    public int NewStockQuantity { get; set; }
    public DateTime ConfirmedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Confirmed";
}
