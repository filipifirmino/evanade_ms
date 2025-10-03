using Inventory.InfraStructure.Rabbit.Subscriber;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Inventory.InfraStructure.Rabbit.BackgroundServices;

public class RabbitMqBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<RabbitMqBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Iniciando RabbitMQ Background Service...");

        try
        {
            using var scope = serviceProvider.CreateScope();
            var orderSubscriber = scope.ServiceProvider.GetRequiredService<OrderSubscriber>();
            
            logger.LogInformation("Iniciando OrderSubscriber para a fila 'order-created-queue'...");
            orderSubscriber.StartListening();
            
            logger.LogInformation("OrderSubscriber iniciado com sucesso!");
            
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao inicializar RabbitMQ Background Service");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Parando RabbitMQ Background Service...");
        await base.StopAsync(cancellationToken);
    }
}
