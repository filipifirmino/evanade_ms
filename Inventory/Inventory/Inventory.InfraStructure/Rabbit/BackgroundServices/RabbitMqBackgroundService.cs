using Inventory.InfraStructure.Rabbit.Subscriber;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Inventory.InfraStructure.Rabbit.BackgroundServices;

public class RabbitMqBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMqBackgroundService> _logger;

    public RabbitMqBackgroundService(
        IServiceProvider serviceProvider, 
        ILogger<RabbitMqBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando RabbitMQ Background Service...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var orderSubscriber = scope.ServiceProvider.GetRequiredService<OrderSubscriber>();
            
            _logger.LogInformation("Iniciando OrderSubscriber para a fila 'order-created-queue'...");
            orderSubscriber.StartListening();
            
            _logger.LogInformation("OrderSubscriber iniciado com sucesso!");

            // Manter o serviço ativo até que o token de cancelamento seja solicitado
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inicializar RabbitMQ Background Service");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parando RabbitMQ Background Service...");
        await base.StopAsync(cancellationToken);
    }
}
