using Inventory.Application.Events.Abstractions;

namespace Inventory.InfraStructure.Rabbit.Messages;

public abstract class BaseSubscriber<T> where T : class {
    private readonly IRabbitMqService _service;

    protected BaseSubscriber(IRabbitMqService service) {
        _service = service;
    }

    protected abstract string QueueName { get; }

    public void StartListening() {
        _service.Subscribe<T>(QueueName, HandleMessage);
    }

    protected abstract void HandleMessage(T message);
}