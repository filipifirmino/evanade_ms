using System.Diagnostics;
using System.Text;

namespace APIGateway.Web.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;

            try
            {
                var requestBody = string.Empty;

                // Registrar detalhes da requisição
                if (context.Request.ContentLength > 0)
                {
                    context.Request.EnableBuffering();
                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                    requestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }

                _logger.LogInformation(
                    "Requisição recebida: {Method} {Path} - Body: {Body}",
                    context.Request.Method,
                    context.Request.Path,
                    requestBody);

                // Usar um MemoryStream para capturar a resposta
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                // Chamar o próximo middleware
                await _next(context);

                // Registrar detalhes da resposta
                stopwatch.Stop();
                responseBody.Seek(0, SeekOrigin.Begin);
                var responseContent = await new StreamReader(responseBody).ReadToEndAsync();
                responseBody.Seek(0, SeekOrigin.Begin);

                _logger.LogInformation(
                    "Resposta enviada: {StatusCode} - Tempo: {ElapsedMs}ms - Body: {Body}",
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    responseContent);

                // Copiar o conteúdo capturado para o stream original
                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante o processamento da requisição");
                throw;
            }
            finally
            {
                // Restaurar o stream original se houver exceção
                context.Response.Body = originalBodyStream;
            }
        }
    }
}