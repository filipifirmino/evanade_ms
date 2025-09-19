using Sales.Application.Entities;

namespace Sales.Application.AbstractionsGateways;

public interface IOrderGateway
{
    Task<Order> AddProduct(Order order);
    Task UpdateOrder(Order order);
    Task DeleteOrder(Order order);
    Task<Order?> GetOrderById(Guid orderId);
    Task<IEnumerable<Order>> GetAllOrders();
}