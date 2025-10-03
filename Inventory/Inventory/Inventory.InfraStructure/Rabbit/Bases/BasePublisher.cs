using Inventory.Application.Events.Abstractions;

namespace Inventory.InfraStructure.Rabbit.Bases;

public abstract class BasePublisher<T> where T : class {
    private readonly IRabbitMqService _service;

    protected BasePublisher(IRabbitMqService service) {
        _service = service;
    }

    protected abstract string QueueName { get; }

    public void Publish(T message) {
        _service.Publish(QueueName, message);
    }
}