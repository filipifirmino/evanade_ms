namespace Sales.Application.AbstractionRabbit;

public interface IMessageConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    bool IsRunning { get; }
}

public interface IMessageConsumer<T> : IMessageConsumer where T : class
{
    Task StartConsumingAsync(string queueName, CancellationToken cancellationToken = default);
    Task StartConsumingAsync(string queueName, string exchange, string routingKey, CancellationToken cancellationToken = default);
}