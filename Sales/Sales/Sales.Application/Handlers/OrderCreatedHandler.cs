using Microsoft.Extensions.Logging;
using Sales.Application.AbstractionRabbit;
using Sales.Application.Events;

namespace Sales.Application.Handlers;

public class OrderCreatedHandler : IMessageHandle<OrderCreated>
{
    private readonly ILogger<OrderCreatedHandler> _logger;

    public OrderCreatedHandler(ILogger<OrderCreatedHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(OrderCreated message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processando evento OrderCreated para pedido {OrderId} do cliente {CustomerId} com valor total {TotalAmount}", 
            message.OrderId, message.CustomerId, message.TotalAmount);
        
        // Aqui você pode implementar a lógica específica para processar o evento OrderCreated
        // Por exemplo: 
        // - Enviar email de confirmação para o cliente
        // - Atualizar cache de produtos
        // - Notificar outros serviços (inventory, payment, etc.)
        // - Registrar métricas de vendas
        
        foreach (var item in message.Items)
        {
            _logger.LogDebug("Item: {ProductName} (ID: {ProductId}) - Quantidade: {Quantity} - Preço: {UnitPrice}", 
                item.ProductName, item.ProductId, item.Quantity, item.UnitPrice);
        }
        
        await Task.Delay(100, cancellationToken); // Simula processamento
        
        _logger.LogInformation("Evento OrderCreated processado com sucesso para pedido {OrderId}", message.OrderId);
    }
}
