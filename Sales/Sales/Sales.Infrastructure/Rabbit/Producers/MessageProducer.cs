using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Sales.Application.AbstractionRabbit;
using Sales.Application.Setings;
using Sales.Infrastructure.Rabbit;

namespace Sales.Infrastructure.Rabbit.Producers;

public class MessageProducer : IMessageProducer
{
    private readonly IRabbitMqConnection _connection;
    private readonly ILogger<MessageProducer> _logger;
    private readonly RabbitMq _settings;

    public MessageProducer(IRabbitMqConnection connection, ILogger<MessageProducer> logger, IOptions<RabbitMq> settings)
    {
        _connection = connection;
        _logger = logger;
        _settings = settings.Value;
    }
    
    public async Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken = default) where T : class
    {
        await PublishAsync(message, string.Empty, queueName, queueName, cancellationToken);
    }
    
    public async Task PublishAsync<T>(T message, string exchange, string routingKey, CancellationToken cancellationToken = default) where T : class
    {
        await PublishAsync(message, exchange, routingKey, string.Empty, cancellationToken);
    }

    public async Task PublishAsync<T>(T message, string exchange, string routingKey, string queueName, CancellationToken cancellationToken = default) where T : class
    {
        using var channel = _connection.CreateChannel();
        
        try
        {
            // Declara o exchange
            channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Direct, durable: true);
            
            // Declara a fila
            channel.QueueDeclare(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            
            // Faz o bind da fila com o exchange
            channel.QueueBind(queue: queueName, exchange: exchange, routingKey: routingKey);

            await PublishMessageAsync(channel, message, exchange, routingKey, cancellationToken);
            
            _logger.LogInformation("Mensagem publicada no exchange {Exchange} com routing key {RoutingKey} para fila {QueueName}", 
                exchange, routingKey, queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar mensagem no exchange {Exchange} com routing key {RoutingKey} para fila {QueueName}", 
                exchange, routingKey, queueName);
            throw;
        }
    }

    private async Task PublishMessageAsync<T>(RabbitMQ.Client.IModel channel, T message, string exchange, string routingKey, CancellationToken cancellationToken) where T : class
    {
        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        var body = Encoding.UTF8.GetBytes(json);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.MessageId = Guid.NewGuid().ToString();
        properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        properties.Type = typeof(T).Name;

        channel.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);

        await Task.CompletedTask;
    }
}
