using Microsoft.Extensions.DependencyInjection;
using Sales.Application.AbstractionsGateways;
using Sales.Infrastructure.Gateways;
using Sales.Infrastructure.Repositories;
using Sales.Infrastructure.Repositories.Abstractions;

namespace Sales.Infrastructure.Configure;

public static class ConfigureInfra
{
    private static void AddGateway(this IServiceCollection services)
    {
        services.AddScoped<IOrderGateway, OrderGateway>();
    }

    private static void AddRepository(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
        services.AddScoped<IOrderRepository, OrderRepository>();
    }

    public static void  AddConfigureInfra(this IServiceCollection services)
    {
        services.AddGateway();
        services.AddRepository();
    }
}