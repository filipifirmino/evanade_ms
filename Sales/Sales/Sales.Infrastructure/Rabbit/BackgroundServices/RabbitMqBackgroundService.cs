using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sales.Application.AbstractionRabbit;
using Sales.Application.Setings;

namespace Sales.Infrastructure.Rabbit.BackgroundServices;

public class RabbitMqBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqBackgroundService> _logger;
    private readonly RabbitMq _settings;
    private readonly List<IMessageConsumer> _consumers = new();

    public RabbitMqBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<RabbitMqBackgroundService> logger,
        IOptions<RabbitMq> settings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando RabbitMQ Background Service");

        try
        {
            await StartConsumersAsync(stoppingToken);
            
            // Mantém o serviço rodando
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                
                // Verifica se os consumers ainda estão rodando
                foreach (var consumer in _consumers)
                {
                    if (!consumer.IsRunning)
                    {
                        _logger.LogWarning("Consumer parou de funcionar, tentando reiniciar...");
                        await consumer.StartAsync(stoppingToken);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("RabbitMQ Background Service foi cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no RabbitMQ Background Service");
        }
        finally
        {
            await StopConsumersAsync();
        }
    }

    private async Task StartConsumersAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        // Mapeamento de filas para tipos de eventos
        var queueTypeMapping = new Dictionary<string, Type>
        {
            { "order-created-queue", typeof(Sales.Application.Events.OrderCreated) }
        };
        
        foreach (var queueConfig in _settings.Queues)
        {
            try
            {
                if (queueTypeMapping.TryGetValue(queueConfig.Name, out var eventType))
                {
                    var consumerType = typeof(IMessageConsumer<>).MakeGenericType(eventType);
                    var consumer = scope.ServiceProvider.GetRequiredService(consumerType);
                    
                    if (consumer != null)
                    {
                        // Usar reflexão para chamar os métodos
                        var startMethod = consumerType.GetMethod("StartAsync");
                        var startConsumingMethod = consumerType.GetMethod("StartConsumingAsync", new[] { typeof(string), typeof(string), typeof(string), typeof(CancellationToken) });
                        
                        if (startMethod != null && startConsumingMethod != null)
                        {
                            await (Task)startMethod.Invoke(consumer, new object[] { cancellationToken })!;
                            await (Task)startConsumingMethod.Invoke(consumer, new object[] { queueConfig.Name, queueConfig.Exchange, queueConfig.RoutingKey, cancellationToken })!;
                            
                            if (consumer is IMessageConsumer messageConsumer)
                            {
                                _consumers.Add(messageConsumer);
                            }
                        }
                        
                        _logger.LogInformation("Consumer {EventType} iniciado para fila {QueueName}", eventType.Name, queueConfig.Name);
                    }
                }
                else
                {
                    _logger.LogWarning("Nenhum mapeamento encontrado para a fila {QueueName}", queueConfig.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar consumer para fila {QueueName}", queueConfig.Name);
            }
        }
    }

    private async Task StopConsumersAsync()
    {
        _logger.LogInformation("Parando consumers...");

        foreach (var consumer in _consumers)
        {
            try
            {
                await consumer.StopAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao parar consumer");
            }
        }

        _consumers.Clear();
        _logger.LogInformation("Todos os consumers foram parados");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parando RabbitMQ Background Service");
        await base.StopAsync(cancellationToken);
    }
}
