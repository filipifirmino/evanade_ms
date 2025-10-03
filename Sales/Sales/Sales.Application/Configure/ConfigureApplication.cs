using Microsoft.Extensions.DependencyInjection;
using Sales.Application.UseCases;
using Sales.Application.UseCases.Abstractions;

namespace Sales.Application.Configure;

public static class ConfigureApplication
{
    private static void ConfigureDependences(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IOrderProcess, OrderProcess>();
        serviceCollection.AddScoped<IOrderConfirmedProcess, OrderConfirmedProcess>();
    }
    
    public static void AddApplicationConfiguration(this IServiceCollection serviceCollection)
    {
        serviceCollection.ConfigureDependences();
    }
}