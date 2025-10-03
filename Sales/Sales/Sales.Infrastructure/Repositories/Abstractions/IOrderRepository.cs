using Sales.Application.Enums;
using Sales.Infrastructure.Entities;

namespace Sales.Infrastructure.Repositories.Abstractions;

public interface IOrderRepository : IRepositoryBase<OrderEntity>
{
    Task UpdateOrderStatus(Guid orderId, Status status);
}