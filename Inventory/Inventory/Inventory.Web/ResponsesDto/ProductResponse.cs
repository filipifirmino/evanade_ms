namespace Inventory.Web.ResponsesDto;

public class ProductResponse
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public Guid ProductId { get; set; }
}