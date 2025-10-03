using Microsoft.Extensions.Logging;
using Sales.Application.AbstractionsGateways;
using Sales.Application.AbstractionRabbit;
using Sales.Application.Entities;
using Sales.Application.Events;
using Sales.Application.UseCases.Abstractions;
using Sales.Application.ValueObject;

namespace Sales.Application.UseCases;

public class OrderProcess : IOrderProcess
{
    private readonly IOrderGateway _orderGateway;
    private readonly ILogger<OrderProcess> _logger;
    private readonly IHttpGateway _httpGateway;
    private readonly IGenericEventProducer _eventProducer;

    public OrderProcess(
        IOrderGateway orderGateway, 
        ILogger<OrderProcess> logger, 
        IHttpGateway httpGateway, 
        IGenericEventProducer eventProducer)
    {
        _orderGateway = orderGateway;
        _logger = logger;
        _httpGateway = httpGateway;
        _eventProducer = eventProducer;
    }

    public async Task<Result<Order>> HandleOrder(Order order, string authorizationToken = null)
    {
        try
        {
            if (!order.IsValid())
            {
                var validationErrors = order.GetValidationErrors();
                _logger.LogWarning("Order validation failed: {ValidationErrors}", validationErrors);
                return Result<Order>.Fail(validationErrors);
            }

            foreach (var product in order.Items)
            {
                var availableQuantity = await _httpGateway.GetProductStockQuantity(product.ProductId, authorizationToken);
                if (availableQuantity >= product.Quantity) continue;
                _logger.LogWarning("Insufficient stock for product {ProductId}. Requested: {Requested}, Available: {Available}", 
                    product.ProductId, product.Quantity, availableQuantity);
                return Result<Order>.Fail($"Insufficient stock for product {product.ProductId}. Requested: {product.Quantity}, Available: {availableQuantity}");
            }
            
            var processedOrder = await _orderGateway.AddProduct(order);
            _logger.LogInformation("Order {OrderId} processed successfully.", order.OrderId);
            
            var orderCreated = new OrderCreated(
                order.OrderId,
                order.CustomerId.ToString(),
                order.TotalAmount,
                order.Items
            );
            
            await _eventProducer.PublishEventAsync(orderCreated);
            return Result<Order>.Success(processedOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order.");
            return Result<Order>.Fail("An error occurred while processing the order.");
        }
    }
}