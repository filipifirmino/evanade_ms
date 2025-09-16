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
        try
        {
            var result = await _orderRepository.AddAsync(order.ToEntity());
            return result.ToDomain();
        }
        catch (SqlException e)
        {
            throw new DataAccessException($"Error inserting record into database ", e);
        }
       
    }

    public async Task UpdateOrder(Order order)
    {
        try
        {
            await _orderRepository.UpdateAsync(order.ToEntity());
        }
        catch (SqlException e)
        {
            throw new DataAccessException($"Error updating record into database ", e);
        }
    }

    public async Task DeleteOrder(Order order)
    {
        try
        {
            await _orderRepository.DeleteAsync(order.ToEntity());
        }
        catch (SqlException e)
        {
            throw new DataAccessException($"Error deleting record into database ", e);
        }
    }

    public async Task<Order?> GetOrderById(Guid orderId)
    {
        try
        {
            var result = await _orderRepository.GetByIdAsync(orderId);
            return result?.ToDomain();
        }
        catch (SqlException e)
        {
            throw new DataAccessException($"Error getting record into database ", e);
        }
    }

    public async Task<IEnumerable<Order>> GetAllOrders()
    {
        try
        {
            var result = await _orderRepository.GetAllAsync();
            return result.Select(x => x.ToDomain());
        }
        catch (SqlException e)
        {
            throw new DataAccessException($"Error getting record into database ", e);
        }
    }
}