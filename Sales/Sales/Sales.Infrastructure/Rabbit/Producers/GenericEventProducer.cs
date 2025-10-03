using Microsoft.Extensions.Logging;
using Sales.Application.AbstractionRabbit;

namespace Sales.Infrastructure.Rabbit.Producers;

public class GenericEventProducer : IGenericEventProducer
{
    private readonly IMessageProducer _messageProducer;
    private readonly ILogger<GenericEventProducer> _logger;
    

    private const string DEFAULT_EXCHANGE = "order-exchange";
    private const string DEFAULT_ROUTING_KEY = "order.created";

    public GenericEventProducer(IMessageProducer messageProducer, ILogger<GenericEventProducer> logger)
    {
        _messageProducer = messageProducer;
        _logger = logger;
    }

    public async Task PublishEventAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            if (eventData is IEventWithQueueConfiguration eventWithConfig)
            {
                _logger.LogInformation("Publicando evento {EventType} na fila {QueueName}", 
                    typeof(T).Name, eventWithConfig.QueueName);

                await _messageProducer.PublishAsync(
                    eventData,
                    DEFAULT_EXCHANGE,
                    DEFAULT_ROUTING_KEY,
                    eventWithConfig.QueueName,
                    cancellationToken);

                _logger.LogInformation("Evento {EventType} publicado com sucesso na fila {QueueName} (Exchange: {Exchange}, RoutingKey: {RoutingKey})", 
                    typeof(T).Name, eventWithConfig.QueueName, DEFAULT_EXCHANGE, DEFAULT_ROUTING_KEY);
            }
            else
            {
                var errorMessage = $"O evento {typeof(T).Name} deve implementar IEventWithQueueConfiguration para ser publicado pelo GenericEventProducer";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento {EventType}", typeof(T).Name);
            throw;
        }
    }
}
