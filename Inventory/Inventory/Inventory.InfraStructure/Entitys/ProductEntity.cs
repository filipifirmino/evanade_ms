namespace Inventory.InfraStructure.Entitys;

public class ProductEntity
{
    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
}