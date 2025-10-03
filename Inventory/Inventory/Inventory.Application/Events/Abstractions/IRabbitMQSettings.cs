namespace Inventory.Application.Events.Abstractions;

public interface IRabbitMqService {
    void Publish<T>(string queueName, T message) where T : class;
    void Subscribe<T>(string queueName, Action<T> onMessage) where T : class;
}