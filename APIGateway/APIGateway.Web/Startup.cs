using APIGateway.Web.Extensions;
using APIGateway.Web.Middleware;
using APIGateway.Infra.Extensions;

namespace APIGateway.Web;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddWebServices(Configuration);
        services.AddInfrastructureServices(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway v1");
                c.RoutePrefix = string.Empty; // Para acessar o Swagger na raiz
            });
        }
        
        app.UseMiddleware<ExceptionHandlingMiddleware>();
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