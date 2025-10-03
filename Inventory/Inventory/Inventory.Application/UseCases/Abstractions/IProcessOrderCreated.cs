using Inventory.Application.Events;

namespace Inventory.Application.UseCases.Abstractions;

public interface IProcessOrderCreated
{
    Task ExecuteAsync(OrderCreatedEvent message);
}