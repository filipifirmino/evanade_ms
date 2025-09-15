using System.Diagnostics;

namespace Inventory.Web.Middlewares;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
         
        context.Response.OnStarting(() =>
        {
            stopwatch.Stop();
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            context.Response.Headers["X-Response-Time-ms"] = elapsedMs.ToString();
            _logger.LogInformation("Request took {ElapsedMs} ms", elapsedMs);

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
