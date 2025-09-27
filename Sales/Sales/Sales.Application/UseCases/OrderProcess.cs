using Microsoft.Extensions.Logging;
using Sales.Application.AbstractionsGateways;
using Sales.Application.AbstractionRabbit;
using Sales.Application.Entities;
using Sales.Application.Events;
using Sales.Application.Mappers;
using Sales.Application.UseCases.Abstractions;
using Sales.Application.ValueObject;

namespace Sales.Application.UseCases;

public class OrderProcess(IOrderGateway orderGateway, ILogger<OrderProcess> logger, 
    IHttpGateway httpGateway, IGenericEventProducer eventProducer) : IOrderProcess
{
    public async Task<Result<Order>> HandleOrder(Order order)
    {
        try
        {
            if (!order.IsValid())
            {
                var validationErrors = order.GetValidationErrors();
                logger.LogWarning("Order validation failed: {ValidationErrors}", validationErrors);
                return Result<Order>.Fail(validationErrors);
            }

            foreach (var product in order.Items)
            {
                var availableQuantity = await httpGateway.GetProductStockQuantity(product.ProductId);
                if (availableQuantity >= product.Quantity) continue;
                logger.LogWarning("Insufficient stock for product {ProductId}. Requested: {Requested}, Available: {Available}", product.ProductId, product.Quantity, availableQuantity);
                return Result<Order>.Fail($"Insufficient stock for product {product.ProductId}. Requested: {product.Quantity}, Available: {availableQuantity}");
            }
            
            var processedOrder = await orderGateway.AddProduct(order);
            logger.LogInformation("Order {OrderId} processed successfully.", order.OrderId);
            
            var orderCreated = new OrderCreated(
                order.OrderId,
                order.CustomerId.ToString(),
                order.TotalAmount,
                order.Items.Select(x => x.ToEvent()).ToList()
            );
            
            await eventProducer.PublishEventAsync(orderCreated);
            return Result<Order>.Success(processedOrder);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing order.");
            return Result<Order>.Fail("An error occurred while processing the order.");
        }
    }
}