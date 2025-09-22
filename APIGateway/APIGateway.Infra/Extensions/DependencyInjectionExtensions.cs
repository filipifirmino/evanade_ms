using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using APIGateway.ApplicationCore.Abstractions;
using APIGateway.ApplicationCore.Services;
using APIGateway.Infra.Authentication;
using APIGateway.Infra.RateLimiting;
using APIGateway.Infra.Http;
using APIGateway.Infra.Configuration;

namespace APIGateway.Infra.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, 
        IConfiguration configuration)
        {
            // Registrar implementações das interfaces do ApplicationCore
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IProxyService, HttpProxyService>();
            services.AddSingleton<IRateLimitService, InMemoryRateLimitService>();
            services.AddSingleton<IRouteConfigurationService, RouteConfigurationService>();
            
            // Configurar HttpClients dinâmicos
            services.AddDynamicHttpClients(configuration);
            
            return services;
        }
        
        private static IServiceCollection AddDynamicHttpClients(
            this IServiceCollection services, IConfiguration configuration)
        {
            var servicesConfig = configuration.GetSection("Services");
            
            foreach (var serviceSection in servicesConfig.GetChildren())
            {
                var serviceName = serviceSection.Key;
                var baseUrl = serviceSection["BaseUrl"];
                var timeout = serviceSection.GetValue<int?>("TimeoutSeconds") ?? 30;
                
                services.AddHttpClient(serviceName, client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                    client.Timeout = TimeSpan.FromSeconds(timeout);
                    client.DefaultRequestHeaders.Add("User-Agent", "ApiGateway-DDD/1.0");
                })
                .AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy());
            }
            
            return services;
        }
        
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => 
                        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
        
        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30));
        }
}