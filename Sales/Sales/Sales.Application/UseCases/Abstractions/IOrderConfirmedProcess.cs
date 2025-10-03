using Sales.Application.Entities;
using Sales.Application.Enums;
using Sales.Application.ValueObject;

namespace Sales.Application.UseCases.Abstractions;

public interface IOrderConfirmedProcess
{
    Task<Result<Order>> HandleOrder(Guid orderId, Status status);
}