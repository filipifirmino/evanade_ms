namespace Inventory.Application.Events.Abstractions;

public interface IRabbitMqService {
    void Publish(string queueName, string message);
    void Subscribe<T>(string queueName, Action<T> onMessage) where T : class;
}