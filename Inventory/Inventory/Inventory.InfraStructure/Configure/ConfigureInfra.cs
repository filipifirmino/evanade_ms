using Inventory.Application.AbstractionsGateways;
using Inventory.Application.Events.Abstractions;
using Inventory.InfraStructure.Gateways;
using Inventory.InfraStructure.Rabbit;
using Inventory.InfraStructure.Rabbit.BackgroundServices;
using Inventory.InfraStructure.Rabbit.Publisher;
using Inventory.InfraStructure.Rabbit.Subscriber;
using Inventory.InfraStructure.Repositories;
using Inventory.InfraStructure.Repositories.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.InfraStructure.Configure;

public static class ConfigureInfra
{
    private static void AddGateway(this IServiceCollection services)
    {
        services.AddScoped<IProductGateway, ProductGateway>();
    }

    private static void AddRepository(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoreBase<>));
        services.AddScoped<IProductRepository, ProductRepository>();
    }

    private static void AddRabbitMq(this IServiceCollection services)
    {
        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        services.AddTransient<OrderConfirmedPublisher>();
        services.AddTransient<OrderSubscriber>();
        services.AddHostedService<RabbitMqBackgroundService>();
    }

    public static void  AddConfigureInfra(this IServiceCollection services)
    {
        services.AddGateway();
        services.AddRepository();
        services.AddRabbitMq();
    }
}