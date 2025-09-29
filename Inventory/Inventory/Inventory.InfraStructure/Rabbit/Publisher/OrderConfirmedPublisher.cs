using Inventory.Application.Events.Abstractions;
using Inventory.InfraStructure.Rabbit.Messages;

namespace Inventory.InfraStructure.Rabbit.Publisher;

public class OrderConfirmedPublisher : BasePublisher
{
    public OrderConfirmedPublisher(IRabbitMqService service) : base(service)
    {
    }
    protected override string QueueName => "order-confirmed";
    
}