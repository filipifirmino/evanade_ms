using Sales.Application.Enums;
using Sales.Infrastructure.Configure;
using Sales.Infrastructure.Entities;
using Sales.Infrastructure.Repositories.Abstractions;

namespace Sales.Infrastructure.Repositories;

public class OrderRepository(DataContext context) 
    : RepositoryBase<OrderEntity>(context), IOrderRepository
{
    public Task UpdateOrderStatus(Guid orderId, Status status)
    {
        var order = context.Find<OrderEntity>(orderId);
        if (order != null)
        {
            order.Status = status;
            context.SaveChanges();
        }
        return Task.CompletedTask;
    }
}