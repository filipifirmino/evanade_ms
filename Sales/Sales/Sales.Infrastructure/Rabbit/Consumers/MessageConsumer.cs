using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sales.Application.AbstractionRabbit;
using Sales.Infrastructure.Rabbit;

namespace Sales.Infrastructure.Rabbit.Consumers;

public class MessageConsumer<T> : IMessageConsumer<T> where T : class
{
    private readonly IRabbitMqConnection _connection;
    private readonly ILogger<MessageConsumer<T>> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private RabbitMQ.Client.IModel? _channel;
    private RabbitMQ.Client.Events.EventingBasicConsumer? _consumer;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly object _lockObject = new();
    private volatile bool _isRunning;

    public MessageConsumer(
        IRabbitMqConnection connection, 
        ILogger<MessageConsumer<T>> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _connection = connection;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public bool IsRunning => _isRunning;
    private string MessageTypeName => typeof(T).Name;

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (!TrySetRunningState(true, "Consumer já está rodando"))
            return Task.CompletedTask;

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _logger.LogInformation("Iniciando consumer para tipo {MessageType}", MessageTypeName);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!TrySetRunningState(false, "Consumer não está rodando"))
            return Task.CompletedTask;

        _cancellationTokenSource?.Cancel();

        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _consumer = null;
            _cancellationTokenSource?.Dispose();
            
            _logger.LogInformation("Consumer parado para tipo {MessageType}", MessageTypeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao parar consumer para tipo {MessageType}", MessageTypeName);
        }

        return Task.CompletedTask;
    }

    private bool TrySetRunningState(bool newState, string warningMessage)
    {
        if (_isRunning == newState)
        {
            _logger.LogWarning(warningMessage);
            return false;
        }

        lock (_lockObject)
        {
            if (_isRunning == newState) return false;
            _isRunning = newState;
        }

        return true;
    }

    public async Task StartConsumingAsync(string queueName, CancellationToken cancellationToken = default)
    {
        await StartConsumingAsync(queueName, string.Empty, queueName, cancellationToken);
    }

    public async Task StartConsumingAsync(string queueName, string exchange, string routingKey, CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
        {
            await StartAsync(cancellationToken);
        }

        try
        {
            _channel = _connection.CreateChannel();
            ConfigureQueue(queueName, exchange, routingKey);
            SetupConsumer(queueName);
            
            _logger.LogInformation("Consumer {MessageType} iniciado para fila {QueueName}", MessageTypeName, queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao iniciar consumer para fila {QueueName}: {ErrorMessage}", queueName, ex.Message);
            throw;
        }
    }

    private void ConfigureQueue(string queueName, string exchange, string routingKey)
    {
        if (_channel == null) return;
        
        // Para inventory-stock-update-confirmed, consumir diretamente da fila (compatibilidade com Inventory)
        if (queueName == "inventory-stock-update-confirmed")
        {
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }
        else if (!string.IsNullOrEmpty(exchange))
        {
            _channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Direct, durable: true);
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: queueName, exchange: exchange, routingKey: routingKey);
        }
        else
        {
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
    }

    private void SetupConsumer(string queueName)
    {
        if (_channel == null) return;
        
        _consumer = new EventingBasicConsumer(_channel);
        _consumer.Received += async (model, ea) =>
        {
            await ProcessMessageAsync(ea);
        };
        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: _consumer);
    }

    private async Task ProcessMessageAsync(BasicDeliverEventArgs ea)
    {
        string messageId = string.Empty;
        
        try
        {
            messageId = ea.BasicProperties.MessageId ?? Guid.NewGuid().ToString();
            
            _logger.LogDebug("Processando mensagem {MessageId} do tipo {MessageType}", 
                messageId, MessageTypeName);

            var body = ea.Body.ToArray();
            var messageJson = Encoding.UTF8.GetString(body);
            
            var message = JsonSerializer.Deserialize<T>(messageJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            });

            if (message == null)
            {
                _logger.LogWarning("Não foi possível deserializar mensagem {MessageId}", messageId);
                RejectMessage(ea.DeliveryTag);
                return;
            }

            // Resolve o handler via DI usando scope
            using var handlerScope = _serviceScopeFactory.CreateScope();
            var handler = handlerScope.ServiceProvider.GetService<IMessageHandle<T>>();
            if (handler == null)
            {
                _logger.LogWarning("Nenhum handler encontrado para tipo {MessageType}", MessageTypeName);
                RejectMessage(ea.DeliveryTag);
                return;
            }

            await handler.HandleAsync(message);
            
            _channel?.BasicAck(ea.DeliveryTag, false);
            
            _logger.LogDebug("Mensagem {MessageId} processada com sucesso", messageId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro de deserialização na mensagem {MessageId}", messageId);
            RejectMessage(ea.DeliveryTag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem {MessageId}", messageId);
            RejectMessage(ea.DeliveryTag);
        }
    }

    private void RejectMessage(ulong deliveryTag)
    {
        _channel?.BasicNack(deliveryTag, false, false);
    }
}
