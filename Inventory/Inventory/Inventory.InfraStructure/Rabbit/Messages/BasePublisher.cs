using Inventory.Application.Events.Abstractions;

namespace Inventory.InfraStructure.Rabbit.Messages;

public abstract class BasePublisher {
    private readonly IRabbitMqService _service;

    protected BasePublisher(IRabbitMqService service) {
        _service = service;
    }

    protected abstract string QueueName { get; }

    public void Publish(string message) {
        _service.Publish(QueueName, message);
    }
}