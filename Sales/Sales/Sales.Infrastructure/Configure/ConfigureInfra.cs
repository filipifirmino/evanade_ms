using Microsoft.Extensions.DependencyInjection;

namespace Sales.Infrastructure.Configure;

public static class ConfigureInfra
{
    public static void AddGateway(this IServiceCollection services)
    {
        // Add gateway services here
    }

    public static void AddRepository(this IServiceCollection services)
    {
        //Add repository services here
    }

    public static void  AddConfigureInfra(this IServiceCollection services)
    {
        services.AddGateway();
        services.AddRepository();
    }
}