using Inventory.Application.Events;

namespace Inventory.Application.Events.Abstractions;

public interface IOrderConfirmedPublisher : IEventPublisher<StockUpdateConfirmedEvent>
{
}
