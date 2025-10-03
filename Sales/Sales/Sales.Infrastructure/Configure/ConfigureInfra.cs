using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Sales.Application.AbstractionsGateways;
using Sales.Application.AbstractionRabbit;
using Sales.Application.Events;
using Sales.Application.Settings;
using Sales.Application.UseCases.Abstractions;
using Sales.Application.UseCases;
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
        services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
        services.AddScoped<IMessageProducer, MessageProducer>();
        services.AddScoped<IGenericEventProducer, GenericEventProducer>();
        services.AddScoped(typeof(IMessageConsumer<>), typeof(MessageConsumer<>));
        
        // Handlers específicos
        services.AddScoped<IMessageHandle<OrderConfirmed>, OrderConfirmedSubscriber>();
        
        services.AddHostedService<RabbitMqBackgroundService>();
    }

    public static void AddConfigureInfra(this IServiceCollection services, IConfiguration configuration)
    {
        // Configurar RabbitMQ
        services.Configure<RabbitMq>(configuration.GetSection(RabbitMq.SectionName));
        
        services.AddGateway();
        services.AddRepository();
        services.AddRabbitMq();
    }
}