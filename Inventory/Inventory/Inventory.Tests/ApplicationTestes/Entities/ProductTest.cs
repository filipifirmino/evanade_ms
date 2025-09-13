using System;
using Inventory.Application.Entities;
using Xunit;

namespace Inventory.Tests.ApplicationTestes.Entities;

public class ProductTest
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateProduct()
    {
        var name = "Produto Teste";
        var description = "Descrição do produto";
        var price = 100.50m;
        var stockQuantity = 10;

        var product = new Product(name, description, price, stockQuantity);

        Assert.NotEqual(Guid.Empty, product.ProductId);
        Assert.Equal(name, product.Name);
        Assert.Equal(description, product.Description);
        Assert.Equal(price, product.Price);
        Assert.Equal(stockQuantity, product.StockQuantity);
    }

    [Fact]
    public void Reserve_WithValidQuantity_ShouldDecreaseStock()
    {
        var product = new Product("Produto", "Descrição", 100m, 10);
        var quantityToReserve = 3;

        product.Reserve(quantityToReserve);

        Assert.Equal(7, product.StockQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Reserve_WithInvalidQuantity_ShouldThrowArgumentException(int invalidQuantity)
    {
        var product = new Product("Produto", "Descrição", 100m, 10);

        var exception = Assert.Throws<ArgumentException>(() => product.Reserve(invalidQuantity));
        Assert.Equal("Quantidade deve ser positiva.", exception.Message);
    }

    [Fact]
    public void Reserve_WithQuantityGreaterThanStock_ShouldThrowInvalidOperationException()
    {
        var product = new Product("Produto", "Descrição", 100m, 5);
        var quantityToReserve = 10;

        var exception = Assert.Throws<InvalidOperationException>(() => product.Reserve(quantityToReserve));
        Assert.Equal("Estoque insuficiente para reserva.", exception.Message);
    }

    [Fact]
    public void Reserve_WithExactStockQuantity_ShouldSucceed()
    {
        var product = new Product("Produto", "Descrição", 100m, 5);
        var quantityToReserve = 5;

        product.Reserve(quantityToReserve);

        Assert.Equal(0, product.StockQuantity);
    }

    [Fact]
    public void Release_WithValidQuantity_ShouldIncreaseStock()
    {
        var product = new Product("Produto", "Descrição", 100m, 10);
        var quantityToRelease = 3;

        product.Release(quantityToRelease);

        Assert.Equal(13, product.StockQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Release_WithInvalidQuantity_ShouldThrowArgumentException(int invalidQuantity)
    {
        var product = new Product("Produto", "Descrição", 100m, 10);

        var exception = Assert.Throws<ArgumentException>(() => product.Release(invalidQuantity));
        Assert.Equal("Quantidade deve ser positiva.", exception.Message);
    }

    [Fact]
    public void AddStock_WithValidQuantity_ShouldIncreaseStock()
    {
        var product = new Product("Produto", "Descrição", 100m, 10);
        var quantityToAdd = 5;

        product.AddStock(quantityToAdd);

        Assert.Equal(15, product.StockQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void AddStock_WithInvalidQuantity_ShouldThrowArgumentException(int invalidQuantity)
    {
        var product = new Product("Produto", "Descrição", 100m, 10);

        var exception = Assert.Throws<ArgumentException>(() => product.AddStock(invalidQuantity));
        Assert.Equal("Quantidade deve ser positiva.", exception.Message);
    }

    [Fact]
    public void SetPrice_WithValidPrice_ShouldUpdatePrice()
    {
        var product = new Product("Produto", "Descrição", 100m, 10);
        var newPrice = 150.75m;

        product.SetPrice(newPrice);

        Assert.Equal(newPrice, product.Price);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void SetPrice_WithNegativePrice_ShouldThrowArgumentException(decimal negativePrice)
    {
        var product = new Product("Produto", "Descrição", 100m, 10);

        var exception = Assert.Throws<ArgumentException>(() => product.SetPrice(negativePrice));
        Assert.Equal("Preço não pode ser negativo.", exception.Message);
    }

    [Fact]
    public void SetPrice_WithZeroPrice_ShouldSucceed()
    {
        var product = new Product("Produto", "Descrição", 100m, 10);
        var zeroPrice = 0m;

        product.SetPrice(zeroPrice);

        Assert.Equal(zeroPrice, product.Price);
    }

    [Fact]
    public void MultipleOperations_ShouldWorkCorrectly()
    {
        var product = new Product("Produto", "Descrição", 100m, 20);

        product.AddStock(10);
        product.Reserve(5);
        product.Release(2);
        product.SetPrice(150m);

        Assert.Equal(27, product.StockQuantity);
        Assert.Equal(150m, product.Price);
    }
}