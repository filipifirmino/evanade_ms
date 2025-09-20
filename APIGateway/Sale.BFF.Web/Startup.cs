using ApiGateway.Extensions;
using ApiGateway.Middleware;


namespace ApiGateway;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        // Configurações da camada Web
        services.AddWebServices(Configuration);
            
        // Configurações da camada Infrastructure
        services.AddInfrastructureServices(Configuration);
            
        // Configurações da camada ApplicationCore são registradas automaticamente via interfaces
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Pipeline de middlewares
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();
            
        app.UseHttpsRedirection();
        app.UseRouting();
            
        app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health");
        });
    }
}