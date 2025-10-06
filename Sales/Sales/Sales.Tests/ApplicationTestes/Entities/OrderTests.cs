using AutoBogus;
using Sales.Application.Entities;
using Sales.Application.ValueObject;

namespace Sales.Tests.ApplicationTestes.Entities;

public class OrderTests
{

    [Fact]
    public void Order_WithValidData_ShouldBeValid()
    {
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 100.50m,
            Items = new List<OrderItem>
            {
                new OrderItem(Guid.NewGuid(), 2, 50.25m)
            }
        };

        var isValid = order.IsValid();

        Assert.True(isValid);
    }

    [Fact]
    public void Order_WithEmptyOrderId_ShouldBeInvalid()
    {
        var order = new Order
        {
            OrderId = Guid.Empty,
            CustomerId = Guid.NewGuid(),
            TotalAmount = 100.50m,
            Items = new List<OrderItem>()
        };

        var isValid = order.IsValid();

        Assert.False(isValid);
    }

    [Fact]
    public void Order_WithEmptyCustomerId_ShouldBeInvalid()
    {
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.Empty,
            TotalAmount = 100.50m,
            Items = new List<OrderItem>()
        };

        var isValid = order.IsValid();

        Assert.False(isValid);
    }

    [Fact]
    public void Order_WithZeroTotalAmount_ShouldBeInvalid()
    {
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 0,
            Items = new List<OrderItem>()
        };

        var isValid = order.IsValid();

        Assert.False(isValid);
    }

    [Fact]
    public void Order_WithNegativeTotalAmount_ShouldBeInvalid()
    {
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = -10.50m,
            Items = new List<OrderItem>()
        };

        var isValid = order.IsValid();

        Assert.False(isValid);
    }

    [Fact]
    public void Order_WithEmptyItems_ShouldBeInvalid()
    {
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 100.50m,
            Items = new List<OrderItem>()
        };

        var isValid = order.IsValid();

        Assert.False(isValid);
    }

    [Fact]
    public void Order_WithInvalidItems_ShouldBeInvalid()
    {
        var order = new Order
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TotalAmount = 100.50m,
            Items = new List<OrderItem>
            {
                new OrderItem(Guid.Empty, 0, -10.50m)
            }
        };

        var isValid = order.IsValid();

        Assert.False(isValid);
    }

    [Fact]
    public void Order_UpdateStatus_ShouldUpdateStatus()
    {
        var order = AutoFaker.Generate<Order>();
        var newStatus = Sales.Application.Enums.Status.Confirmed;

        order.Status = newStatus;

        Assert.Equal(newStatus, order.Status);
    }
}
