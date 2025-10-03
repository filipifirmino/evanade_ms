namespace Sales.Application.AbstractionRabbit;

public interface IMessageProducer
{
    Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken = default) where T : class;
    Task PublishAsync<T>(T message, string exchange, string routingKey, CancellationToken cancellationToken = default) where T : class;
    Task PublishAsync<T>(T message, string exchange, string routingKey, string queueName, CancellationToken cancellationToken = default) where T : class;
}