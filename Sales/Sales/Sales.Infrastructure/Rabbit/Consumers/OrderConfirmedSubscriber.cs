using Microsoft.Extensions.Logging;
using Sales.Application.AbstractionRabbit;
using Sales.Application.Events;
using Sales.Application.UseCases.Abstractions;

namespace Sales.Infrastructure.Rabbit.Consumers;

public class OrderConfirmedSubscriber : IMessageHandle<OrderConfirmed>
{
    private readonly ILogger<OrderConfirmedSubscriber> _logger;
    private readonly IOrderConfirmedProcess _orderConfirmedProcess;

    public OrderConfirmedSubscriber(
        ILogger<OrderConfirmedSubscriber> logger,
        IOrderConfirmedProcess orderProcess)
    {
        _logger = logger;
        _orderConfirmedProcess = orderProcess;
    }

    public async Task HandleAsync(OrderConfirmed message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processando OrderConfirmed para pedido {OrderId} com status {Status}", 
            message.OrderId, message.Status);
            
        try
        {
            await _orderConfirmedProcess.HandleOrder(message.OrderId, message.Status);
            _logger.LogInformation("OrderConfirmed processado com sucesso para pedido {OrderId}", message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar OrderConfirmed para pedido {OrderId}", message.OrderId);
            throw;
        }
    }
}
