using Inventory.Application.UseCases;
using Inventory.Application.UseCases.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Application.Configure;

public static class ConfigureApplication
{
    private static void ConfigureDependences(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IProcessOrderCreated, ProcessOrderCreated>();
    }
    
    public static void AddApplicationConfiguration(this IServiceCollection serviceCollection)
    {
        serviceCollection.ConfigureDependences();
    }
}