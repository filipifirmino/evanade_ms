using Inventory.Application.Events;
using Inventory.Application.Events.Abstractions;
using Inventory.InfraStructure.Rabbit.Bases;

namespace Inventory.InfraStructure.Rabbit.Publisher;

public class OrderConfirmedPublisher(IRabbitMqService service)
    : BasePublisher<StockUpdateConfirmedEvent>(service), IOrderConfirmedPublisher
{
    protected override string QueueName => "inventory-stock-update-confirmed";
}