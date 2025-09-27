using Microsoft.Extensions.DependencyInjection;
using Sales.Application.AbstractionsGateways;
using Sales.Application.AbstractionRabbit;
using Sales.Application.Handlers;
using Sales.Application.Setings;
using Sales.Infrastructure.Gateways;
using Sales.Infrastructure.Rabbit;
using Sales.Infrastructure.Rabbit.BackgroundServices;
using Sales.Infrastructure.Rabbit.Consumers;
using Sales.Infrastructure.Rabbit.Producers;
using Sales.Infrastructure.Repositories;
using Sales.Infrastructure.Repositories.Abstractions;

namespace Sales.Infrastructure.Configure;

public static class ConfigureInfra
{
    private static void AddGateway(this IServiceCollection services)
    {
        services.AddScoped<IOrderGateway, OrderGateway>();
        services.AddScoped<IHttpGateway, HttpGateway>();
    }

    private static void AddRepository(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
        services.AddScoped<IOrderRepository, OrderRepository>();
    }

    private static void AddRabbitMq(this IServiceCollection services)
    {
        // Conexão RabbitMQ
        services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();

        // Message Producer
        services.AddScoped<IMessageProducer, MessageProducer>();
        
        // Generic Event Producer
        services.AddScoped<IGenericEventProducer, GenericEventProducer>();


        // Message Consumers (genéricos)
        services.AddScoped(typeof(IMessageConsumer<>), typeof(MessageConsumer<>));

        // Handlers específicos
        services.AddScoped<IMessageHandle<Sales.Application.Events.OrderCreated>, OrderCreatedHandler>();

        // Background Service para gerenciar consumers
        services.AddHostedService<RabbitMqBackgroundService>();
    }

    public static void  AddConfigureInfra(this IServiceCollection services)
    {
        services.AddGateway();
        services.AddRepository();
        services.AddRabbitMq();
    }
}