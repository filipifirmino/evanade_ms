using Microsoft.Data.SqlClient;
using Sales.Application.AbstractionsGateways;
using Sales.Application.Entities;
using Sales.Infrastructure.Mappers;
using Sales.Infrastructure.Repositories.Abstractions;
using Sales.Infrastructure.Tools;

namespace Sales.Infrastructure.Gateways;

public class OrderGateway : IOrderGateway
{
    private readonly IOrderRepository _orderRepository;
    
    public OrderGateway(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Order> AddProduct(Order order)
    {
        return await ExecuteDatabaseOperation(
            async () => (await _orderRepository.AddAsync(order.ToEntity())).ToDomain(),
            "Error creating order"
        );
    }

    public async Task UpdateOrder(Order order)
    {
        await ExecuteDatabaseOperation(
            () => _orderRepository.UpdateAsync(order.ToEntity()),
            "Error updating order"
        );
    }

    public async Task DeleteOrder(Order order)
    {
        await ExecuteDatabaseOperation(
            () => _orderRepository.DeleteAsync(order.ToEntity()),
            "Error deleting order"
        );
    }

    public async Task<Order?> GetOrderById(Guid orderId)
    {
        return await ExecuteDatabaseOperation(
            async () => (await _orderRepository.GetByIdAsync(orderId))?.ToDomain(),
            "Error retrieving order"
        );
    }

    public async Task<IEnumerable<Order>> GetAllOrders()
    {
        return await ExecuteDatabaseOperation(
            async () => (await _orderRepository.GetAllAsync()).Select(x => x.ToDomain()),
            "Error retrieving orders"
        );
    }

    private async Task<T> ExecuteDatabaseOperation<T>(Func<Task<T>> operation, string errorMessage)
    {
        try
        {
            return await operation();
        }
        catch (SqlException ex)
        {
            throw new DataAccessException(errorMessage, ex);
        }
    }

    private async Task ExecuteDatabaseOperation(Func<Task> operation, string errorMessage)
    {
        try
        {
            await operation();
        }
        catch (SqlException ex)
        {
            throw new DataAccessException(errorMessage, ex);
        }
    }
}