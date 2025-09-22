using APIGateway.ApplicationCore.Abstractions;

namespace APIGateway.Web.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRateLimitService _rateLimitService;
        private readonly ILogger<RateLimitingMiddleware> _logger;

        public RateLimitingMiddleware(
            RequestDelegate next,
            IRateLimitService rateLimitService,
            ILogger<RateLimitingMiddleware> logger)
        {
            _next = next;
            _rateLimitService = rateLimitService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            
            string category = "general";
            
            // Identificar categoria com base no caminho
            if (path?.Contains("/api/auth/") == true)
            {
                category = "auth";
            }
            else if (path?.Contains("/api/inventory/") == true)
            {
                category = "inventory";
            }
            else if (path?.Contains("/api/sales/") == true)
            {
                category = "sales";
            }
            
            // Verificar se o limite foi excedido
            if (!await _rateLimitService.AllowRequestAsync(category, ipAddress))
            {
                _logger.LogWarning(
                    "Limite de requisições excedido para categoria {Category} e IP {IpAddress}",
                    category, ipAddress);
                
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.Headers.Add("Retry-After", "60");
                await context.Response.WriteAsync(
                    "Muitas requisições. Por favor, tente novamente em alguns instantes.");
                
                return;
            }
            
            await _next(context);
        }
    }
}