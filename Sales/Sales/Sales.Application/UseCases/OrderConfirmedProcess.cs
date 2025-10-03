using Microsoft.Extensions.Logging;
using Sales.Application.AbstractionsGateways;
using Sales.Application.Entities;
using Sales.Application.Enums;
using Sales.Application.UseCases.Abstractions;
using Sales.Application.ValueObject;

namespace Sales.Application.UseCases;

public class OrderConfirmedProcess: IOrderConfirmedProcess
{
    private readonly IOrderGateway _orderGateway;
    private readonly ILogger<OrderConfirmedProcess> _logger;
    public OrderConfirmedProcess(IOrderGateway orderGateway, ILogger<OrderConfirmedProcess> logger)
    {
        _logger = logger;
        _orderGateway = orderGateway;
    }
    public async Task<Result<Order>> HandleOrder(Guid orderId, Status status)
    {
        _logger.LogInformation("Handling order confirmation for OrderId: {OrderId} with Status: {Status}", orderId, status);
        try
        {
            await _orderGateway.UpdateOrderStatus(orderId, status);
            return Result<Order>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updated status order");
            throw;
        }
    }
}