namespace Inventory.InfraStructure.Entities;

public class ProductEntity
{
    public Guid ProductId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get;  set; }
    public int StockQuantity { get; set; }
}