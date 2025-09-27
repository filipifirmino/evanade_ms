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
    private readonly IServiceProvider _serviceProvider;
    private RabbitMQ.Client.IModel? _channel;
    private RabbitMQ.Client.Events.EventingBasicConsumer? _consumer;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly object _lockObject = new();
    private volatile bool _isRunning;

    public MessageConsumer(
        IRabbitMqConnection connection, 
        ILogger<MessageConsumer<T>> logger,
        IServiceProvider serviceProvider)
    {
        _connection = connection;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public bool IsRunning => _isRunning;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            _logger.LogWarning("Consumer já está rodando");
            return;
        }

        lock (_lockObject)
        {
            if (_isRunning) return;
            
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _isRunning = true;
        }

        _logger.LogInformation("Iniciando consumer para tipo {MessageType}", typeof(T).Name);
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
        {
            _logger.LogWarning("Consumer não está rodando");
            return;
        }

        lock (_lockObject)
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            _cancellationTokenSource?.Cancel();
        }

        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _consumer = null;
            _cancellationTokenSource?.Dispose();
            
            _logger.LogInformation("Consumer parado para tipo {MessageType}", typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao parar consumer para tipo {MessageType}", typeof(T).Name);
        }

        await Task.CompletedTask;
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

            // Configura exchange se fornecido
            if (!string.IsNullOrEmpty(exchange))
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

            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += async (model, ea) =>
            {
                await ProcessMessageAsync(ea);
            };

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer: _consumer);

            _logger.LogInformation("Consumer iniciado para fila {QueueName} com tipo {MessageType}", 
                queueName, typeof(T).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar consumer para fila {QueueName}", queueName);
            throw;
        }

        await Task.CompletedTask;
    }

    private async Task ProcessMessageAsync(BasicDeliverEventArgs ea)
    {
        string messageId = string.Empty;
        
        try
        {
            messageId = ea.BasicProperties.MessageId ?? Guid.NewGuid().ToString();
            
            _logger.LogDebug("Processando mensagem {MessageId} do tipo {MessageType}", 
                messageId, typeof(T).Name);

            var body = ea.Body.ToArray();
            var messageJson = Encoding.UTF8.GetString(body);
            
            var message = JsonSerializer.Deserialize<T>(messageJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (message == null)
            {
                _logger.LogWarning("Não foi possível deserializar mensagem {MessageId}", messageId);
                _channel?.BasicNack(ea.DeliveryTag, false, false);
                return;
            }

            // Resolve o handler via DI
            var handler = _serviceProvider.GetService<IMessageHandle<T>>();
            if (handler == null)
            {
                _logger.LogWarning("Nenhum handler encontrado para tipo {MessageType}", typeof(T).Name);
                _channel?.BasicNack(ea.DeliveryTag, false, false);
                return;
            }

            await handler.HandleAsync(message);
            
            _channel?.BasicAck(ea.DeliveryTag, false);
            
            _logger.LogDebug("Mensagem {MessageId} processada com sucesso", messageId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erro de deserialização na mensagem {MessageId}", messageId);
            _channel?.BasicNack(ea.DeliveryTag, false, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem {MessageId}", messageId);
            
            // Rejeita a mensagem e não recoloca na fila
            _channel?.BasicNack(ea.DeliveryTag, false, false);
        }
    }
}
