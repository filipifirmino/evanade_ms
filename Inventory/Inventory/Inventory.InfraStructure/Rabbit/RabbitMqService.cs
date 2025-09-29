using System;
using System.Text;
using System.Text.Json;
using Inventory.Application.Events.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Inventory.InfraStructure.Rabbit;

public class RabbitMqService : IRabbitMqService, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed;

    public RabbitMqService(IOptions<RabbitMqSettings> options, ILogger<RabbitMqService> logger)
    {
        _settings = options.Value;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    private void EnsureConnection()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                Port = _settings.Port,
                AutomaticRecoveryEnabled = _settings.AutomaticRecoveryEnabled,
                NetworkRecoveryInterval = _settings.NetworkRecoveryInterval
            };
            _connection = factory.CreateConnection();
        }
        if (_channel == null || !_channel.IsOpen)
        {
            _channel = _connection.CreateModel();
        }
    }

    public void Publish(string queueName, string message)
    {
        try
        {
            EnsureConnection();
            _channel!.QueueDeclare(queueName, true, false, false, null);
            var body = Encoding.UTF8.GetBytes(message);
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            _channel.BasicPublish("", queueName, properties, body);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao publicar mensagem: {ex.Message}", ex);
        }
    }


    public void Subscribe<T>(string queueName, Action<T> onMessage) where T : class
    {
        try
        {
            EnsureConnection();
            _channel!.QueueDeclare(queueName, true, false, false, null);
            
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var messageJson = Encoding.UTF8.GetString(body);
                    
                    _logger.LogDebug("Mensagem JSON recebida: {MessageJson}", messageJson);
                    
                    var message = JsonSerializer.Deserialize<T>(messageJson, _jsonOptions);
                    
                    if (message != null)
                    {
                        _logger.LogDebug("Mensagem deserializada com sucesso para o tipo {Type}", typeof(T).Name);
                        onMessage(message);
                    }
                    else
                    {
                        _logger.LogWarning("Falha ao deserializar mensagem - resultado nulo");
                    }
                    
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Erro de deserialização JSON: {MessageJson}", Encoding.UTF8.GetString(ea.Body.ToArray()));
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem tipada: {MessageJson}", Encoding.UTF8.GetString(ea.Body.ToArray()));
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                }
            };
            
            _channel.BasicConsume(queueName, false, consumer);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao configurar subscriber tipado: {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            _disposed = true;
        }
    }
}