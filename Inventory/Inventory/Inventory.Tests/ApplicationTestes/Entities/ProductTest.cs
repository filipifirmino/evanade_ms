using AutoBogus;
using Inventory.Application.Entities;
namespace Inventory.Tests.ApplicationTestes.Entities;

public class ProductTest
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateProduct()
    {
        var productTest = AutoFaker.Generate<Product>();

        var product = productTest;

        Assert.NotEqual(Guid.Empty, product.ProductId);
        Assert.Equal(product.Name, product.Name);
        Assert.Equal(product.Description, product.Description);
        Assert.Equal(product.Price, product.Price);
        Assert.Equal(product.StockQuantity, product.StockQuantity);
    }

    [Fact]
    public void Reserve_WithValidQuantity_ShouldDecreaseStock()
    {
        var product = new AutoFaker<Product>()
            .RuleFor(p => p.StockQuantity, 10)
            .Generate();
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
        var product =  new AutoFaker<Product>()
            .RuleFor(p => p.StockQuantity, 10)
            .Generate();

        var exception = Assert.Throws<ArgumentException>(() => product.Reserve(invalidQuantity));
        Assert.Equal("Quantidade deve ser positiva.", exception.Message);
    }

    [Fact]
    public void Reserve_WithQuantityGreaterThanStock_ShouldThrowInvalidOperationException()
    {
        var product = new AutoFaker<Product>()
            .RuleFor(p => p.StockQuantity, 5)
            .Generate();
        var quantityToReserve = 10;

        var exception = Assert.Throws<InvalidOperationException>(() => product.Reserve(quantityToReserve));
        Assert.Equal("Estoque insuficiente para reserva.", exception.Message);
    }

    [Fact]
    public void Reserve_WithExactStockQuantity_ShouldSucceed()
    {
        var product = new AutoFaker<Product>()
            .RuleFor(p => p.StockQuantity, 5)
            .Generate();
        var quantityToReserve = 5;

        product.Reserve(quantityToReserve);

        Assert.Equal(0, product.StockQuantity);
    }

    [Fact]
    public void Release_WithValidQuantity_ShouldIncreaseStock()
    {
        var product = new AutoFaker<Product>()
            .RuleFor(p => p.StockQuantity, 10)
            .Generate();
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
        var product = new AutoFaker<Product>()
            .RuleFor(p => p.StockQuantity, 10)
            .Generate();

        var exception = Assert.Throws<ArgumentException>(() => product.Release(invalidQuantity));
        Assert.Equal("Quantidade deve ser positiva.", exception.Message);
    }

    [Fact]
    public void AddStock_WithValidQuantity_ShouldIncreaseStock()
    {
        var product = new AutoFaker<Product>()
            .RuleFor(p => p.StockQuantity, 10)
            .Generate();
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
        var product = new AutoFaker<Product>()
            .RuleFor(p => p.StockQuantity, 10)
            .Generate();

        var exception = Assert.Throws<ArgumentException>(() => product.AddStock(invalidQuantity));
        Assert.Equal("Quantidade deve ser positiva.", exception.Message);
    }

    [Fact]
    public void SetPrice_WithValidPrice_ShouldUpdatePrice()
    {
        var product = new AutoFaker<Product>()
            .RuleFor(p => p.Price, 100m)
            .Generate();
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
        var product = AutoFaker.Generate<Product>();

        var exception = Assert.Throws<ArgumentException>(() => product.SetPrice(negativePrice));
        Assert.Equal("Preço não pode ser negativo.", exception.Message);
    }

    [Fact]
    public void SetPrice_WithZeroPrice_ShouldSucceed()
    {
        var product = AutoFaker.Generate<Product>();
        var zeroPrice = 0m;

        product.SetPrice(zeroPrice);

        Assert.Equal(zeroPrice, product.Price);
    }

    [Fact]
    public void MultipleOperations_ShouldWorkCorrectly()
    {
        var product = new AutoFaker<Product>()
            .RuleFor(p => p.StockQuantity, 20)
            .Generate();;

        product.AddStock(10);
        product.Reserve(5);
        product.Release(2);
        product.SetPrice(150m);

        Assert.Equal(27, product.StockQuantity);
        Assert.Equal(150m, product.Price);
    }
}