using Inventory.Application.AbstractionsGateways;
using Inventory.Application.Events;
using Inventory.Application.Events.Abstractions;
using Inventory.Application.UseCases.Abstractions;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.UseCases;

public class ProcessOrderCreated: IProcessOrderCreated
{
    private readonly IProductGateway _productGateway;
    private readonly ILogger<ProcessOrderCreated> _logger;
    private readonly IOrderConfirmedPublisher _orderConfirmedPublisher;
    
    public ProcessOrderCreated(
        IProductGateway productGateway, 
        ILogger<ProcessOrderCreated> logger,
        IOrderConfirmedPublisher orderConfirmedPublisher)
    {
        _productGateway = productGateway;
        _logger = logger;
        _orderConfirmedPublisher = orderConfirmedPublisher;
    }
    public async Task ExecuteAsync(OrderCreatedEvent @event)
    {
        _logger.LogInformation("Message received in 'ProcessOrderCreated' use case: {OrderItems}", @event.Items);
        List<Task> tasks = new List<Task>();
        
        foreach (var orderItem in @event.Items)
        {
            var product = await _productGateway.GetProductById(orderItem.ProductId);
            if (product != null)
            {
                var newQuantity = product.StockQuantity - orderItem.Quantity;
                var result = _productGateway.UpdateQuantityProduct(newQuantity, orderItem.ProductId);
                tasks.Add(result);                
            }
            else
            {
                _logger.LogWarning("Produto não encontrado: {ProductId}", orderItem.ProductId);
            }
            var stockUpdateEvent = new StockUpdateConfirmedEvent
                {
                    OrderId = @event.OrderId,
                    ProductId = orderItem.ProductId,
                    ProductName = orderItem.ProductName,
                    QuantityReserved = orderItem.Quantity,
                    NewStockQuantity = @event.Items.Sum(x => x.Quantity),
                    ConfirmedAt = DateTime.UtcNow,
                    Status = "Confirmed"
                };
                
                _orderConfirmedPublisher.Publish(stockUpdateEvent);
                _logger.LogInformation("StockUpdateConfirmedEvent publicado para produto {ProductId}", orderItem.ProductId);
        }
        
        await Task.WhenAll(tasks);
        _logger.LogInformation("Message processed in 'ProcessOrderCreated' use case");
    }
}