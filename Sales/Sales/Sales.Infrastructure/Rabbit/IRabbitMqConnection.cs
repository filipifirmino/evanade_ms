using RabbitMQ.Client;

namespace Sales.Infrastructure.Rabbit;

public interface IRabbitMqConnection : IDisposable
{
    IConnection Connection { get; }
    RabbitMQ.Client.IModel CreateChannel();
    bool IsConnected { get; }
    void EnsureConnected();
}