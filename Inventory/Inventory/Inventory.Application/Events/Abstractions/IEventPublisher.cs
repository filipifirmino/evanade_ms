namespace Inventory.Application.Events.Abstractions;

public interface IEventPublisher<T> where T : class
{
    void Publish(T message);
}
