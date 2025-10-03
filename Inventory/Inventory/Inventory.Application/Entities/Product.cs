using System;

namespace Inventory.Application.Entities;

public class Product
{
    public Guid ProductId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int Reservation { get; set; }

    public Product(string name, string description, decimal price, int stockQuantity)
    {
        ProductId = Guid.NewGuid();
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
    }

    public int GetStockAvailable()
    {
        return StockQuantity - Reservation;
    }
    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantidade deve ser positiva.");
        if (quantity > StockQuantity)
            throw new InvalidOperationException("Estoque insuficiente para reserva.");
        StockQuantity -= quantity;
    }

    public void Release(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantidade deve ser positiva.");
        StockQuantity += quantity;
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantidade deve ser positiva.");
        StockQuantity += quantity;
    }

    public void SetPrice(decimal price)
    {
        if (price < 0)
            throw new ArgumentException("Preço não pode ser negativo.");
        Price = price;
    }
}