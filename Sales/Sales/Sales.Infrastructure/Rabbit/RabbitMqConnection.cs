using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Sales.Application.Setings;
using Sales.Infrastructure.Rabbit;

namespace Sales.Infrastructure.Rabbit;

public class RabbitMqConnection: IRabbitMqConnection, IDisposable
{
    private readonly RabbitMq _settings;
    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqConnection(IOptions<RabbitMq> settings)
    {
        _settings = settings.Value;
        CreateConnection();
    }
    
    public IConnection Connection => _connection ?? throw new InvalidOperationException("RabbitMQ connection not established");

    public bool IsConnected => _connection?.IsOpen == true;

    public void EnsureConnected()
    {
        if (!IsConnected)
        {
            CreateConnection();
        }
    }

    public RabbitMQ.Client.IModel CreateChannel()
    {
        EnsureConnected();
        return Connection.CreateModel();
    }

    private void CreateConnection()
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
            AutomaticRecoveryEnabled = _settings.AutomaticRecoveryEnabled,
            NetworkRecoveryInterval = _settings.NetworkRecoveryInterval,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _connection?.Dispose();
            _disposed = true;
        }
    }
}