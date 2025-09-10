using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Application.Configure;

public static class ConfigureApplication
{
    private static void ConfigureDependences(this IServiceCollection serviceCollection)
    {
        //TODO: Adicionar injeções de dependência da camada Application
    }
    
    public static void AddApplicationConfiguration(this IServiceCollection serviceCollection)
    {
        serviceCollection.ConfigureDependences();
    }
}