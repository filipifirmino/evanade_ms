using Inventory.Application.AbstractionsGateways;
using Inventory.InfraStructure.Gateways;
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

    public static void  AddConfigureInfra(this IServiceCollection services)
    {
        services.AddGateway();
        services.AddRepository();
    }
}