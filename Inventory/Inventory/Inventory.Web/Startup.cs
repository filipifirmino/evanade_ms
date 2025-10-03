using Inventory.Application.Configure;
using Inventory.InfraStructure.Configure;
using Inventory.InfraStructure.Rabbit;
using Inventory.Web.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace Inventory.Web;

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
        services.Configure<RabbitMqSettings>(_Configuration.GetSection("RabbitMQ"));
        services.AddConfigureInfra();
        services.AddApplicationConfiguration();
        services.AddSwaggerGen(s =>
        {
            s.SwaggerDoc("v1", new OpenApiInfo { Title = "Inventory API", Version = "v1" });
        });
        services.AddDbContext<DataContext>(options =>
            options.UseSqlServer(_Configuration.GetConnectionString("DefaultConnection")));
        
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory API v1"));
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