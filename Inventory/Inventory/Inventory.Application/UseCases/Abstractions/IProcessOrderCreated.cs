using Inventory.Application.Events;

namespace Inventory.Application.UseCases.Abstractions;

public interface IProcessOrderCreated
{
    Task ExectuteAsync(List<OrderItemEvent> orderItems);
}