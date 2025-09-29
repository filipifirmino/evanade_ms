using System.Threading.Tasks;
using Inventory.Application.Events;
using Inventory.Application.Events.Abstractions;
using Inventory.Application.UseCases.Abstractions;
using Inventory.InfraStructure.Rabbit.Messages;
using Microsoft.Extensions.Logging;

namespace Inventory.InfraStructure.Rabbit.Subscriber;

public class OrderSubscriber : BaseSubscriber<OrderCreatedEvent>
{
    private readonly ILogger<OrderSubscriber> _logger;
    private readonly IProcessOrderCreated _processOrderCreated;

    public OrderSubscriber(IRabbitMqService service, ILogger<OrderSubscriber> logger, IProcessOrderCreated processOrderCreated): base(service)
    {
        _logger = logger;
        _processOrderCreated = processOrderCreated;
    }
    
    protected override string QueueName => "order-created-queue";
    
    protected override void HandleMessage(OrderCreatedEvent message)
    {
        try
        {
            _logger.LogInformation("Mensagem recebida na fila 'order-created-queue': {message}", @message);
            Console.WriteLine($"Pedido recebido: {message}");

            // Executar de forma assíncrona sem bloquear
            Task.Run(async () => await _processOrderCreated.ExectuteAsync(message.Items));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem da fila 'order-created-queue': {Message}", message);
            throw;
        }
    }
}