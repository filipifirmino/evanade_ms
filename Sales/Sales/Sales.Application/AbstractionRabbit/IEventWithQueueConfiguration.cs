namespace Sales.Application.AbstractionRabbit;

public interface IEventWithQueueConfiguration
{
    string QueueName { get; }
}
