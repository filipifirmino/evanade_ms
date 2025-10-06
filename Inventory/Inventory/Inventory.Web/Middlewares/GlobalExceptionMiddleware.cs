using System.Text.Json;
using Inventory.InfraStructure.Tools;
using Microsoft.AspNetCore.Http;

namespace Inventory.Web.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var (statusCode, message) = exception switch
        {
            ArgumentException => (400, "Parâmetro inválido: " + exception.Message),
            InvalidOperationException => (400, "Operação inválida: " + exception.Message),
            DataAccessException => (500, "Erro de acesso aos dados: " + exception.Message),
            UnauthorizedAccessException => (401, "Acesso não autorizado"),
            _ => (500, "Erro interno do servidor")
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            error = message,
            statusCode = statusCode,
            timestamp = DateTime.UtcNow,
            path = context.Request.Path
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
