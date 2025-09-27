using Sales.Application.Entities;
using Sales.Application.ValueObject;

namespace Sales.Application.UseCases.Abstractions;

public interface IOrderProcess
{
    Task<Result<Order>> HandleOrder(Order order);
}