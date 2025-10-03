using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Sales.Application.Configure;
using Sales.Infrastructure.Configure;
using Sales.Web.Extensions;
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
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
        services.AddJwtAuthentication(_Configuration);
        services.AddConfigureInfra(_Configuration);
        services.AddApplicationConfiguration();
        services.AddHttpClient();
        services.AddSwaggerGen(s =>
        {
            s.SwaggerDoc("v1", new OpenApiInfo { Title = "Sales API", Version = "v1" });
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
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sales API v1"));
        }
        
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<RequestTimingMiddleware>();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}