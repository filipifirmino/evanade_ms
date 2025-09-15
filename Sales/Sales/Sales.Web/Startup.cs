using Sales.Application.Configure;
using Sales.Infrastructure.Configure;
using Sales.Web.Middlewares;

namespace Sales.Web;

public class Startup
{
    public  IConfiguration _Configuration { get; }
    
    public Startup(IConfiguration configuration)
    {
        _Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddConfigureInfra();
        services.AddApplicationConfiguration();
        services.AddSwaggerGen(s =>
        {
            s.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Sales API", Version = "v1" });
        });
        services.AddDbContext<DataContext>();
    }
    
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sales API v1"));
        }
        
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseMiddleware<RequestTimingMiddleware>();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}