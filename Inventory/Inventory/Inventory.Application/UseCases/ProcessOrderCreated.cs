using Inventory.Application.AbstractionsGateways;
using Inventory.Application.Events;
using Inventory.Application.UseCases.Abstractions;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.UseCases;

public class ProcessOrderCreated: IProcessOrderCreated
{
    private readonly IProductGateway _productGateway;
    private readonly ILogger<ProcessOrderCreated> _logger;
    public ProcessOrderCreated(IProductGateway productGateway, ILogger<ProcessOrderCreated> logger)
    {
        _productGateway = productGateway;
        _logger = logger;
    }
    public async Task ExectuteAsync(List<OrderItemEvent> orderItems)
    {
        _logger.LogInformation("Message received in 'ProcessOrderCreated' use case: {OrderItems}", orderItems);
        List<Task> tasks = new List<Task>();
        
        foreach (var orderItem in orderItems)
        {
            var product = await _productGateway.GetProductById(orderItem.ProductId);
            if (product != null)
            {
                // Reduzir o estoque pela quantidade do pedido
                var newQuantity = product.StockQuantity - orderItem.Quantity;
                var result = _productGateway.UpdateQuantityProduct(newQuantity, orderItem.ProductId);
                tasks.Add(result);
            }
            else
            {
                _logger.LogWarning("Produto não encontrado: {ProductId}", orderItem.ProductId);
            }

            //TODO: Publicar evento de quantidade atualizada para confirmar a compra no serviço de compra.
        }
        
        await Task.WhenAll(tasks);
        _logger.LogInformation("Message processed in 'ProcessOrderCreated' use case");
    }
}