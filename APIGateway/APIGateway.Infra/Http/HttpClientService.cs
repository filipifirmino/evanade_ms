using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using APIGateway.ApplicationCore.Abstractions;
using APIGateway.ApplicationCore.DTOs;

namespace APIGateway.Infra.Http
{
    public class HttpProxyService : IProxyService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HttpProxyService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IRouteConfigurationService _routeConfigurationService;

        public HttpProxyService(
            IHttpClientFactory httpClientFactory,
            ILogger<HttpProxyService> logger,
            IConfiguration configuration,
            IRouteConfigurationService routeConfigurationService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
            _routeConfigurationService = routeConfigurationService;
        }

        public async Task<ProxyResponse> ForwardRequestAsync(
            string serviceName, string path, HttpMethod method, object? body = null, 
            Dictionary<string, string>? headers = null)
        {
            try
            {
                // Verificar se o serviço está configurado
                if (!_routeConfigurationService.IsServiceConfigured(serviceName))
                {
                    _logger.LogWarning("Serviço não configurado: {ServiceName}", serviceName);
                    return new ProxyResponse
                    {
                        StatusCode = 404,
                        Content = $"Serviço '{serviceName}' não encontrado"
                    };
                }

                // Verificar se a rota é permitida
                if (!_routeConfigurationService.IsRouteAllowed(serviceName, path, method))
                {
                    _logger.LogWarning("Rota não permitida: {ServiceName} - {Path} - {Method}", 
                        serviceName, path, method);
                    return new ProxyResponse
                    {
                        StatusCode = 403,
                        Content = "Rota não autorizada"
                    };
                }
                
                var client = _httpClientFactory.CreateClient(serviceName);
                // Montar caminho downstream usando DownstreamBasePath
                var downstreamBasePath = _routeConfigurationService.GetDownstreamBasePath(serviceName);
                var relativePath = path;
                
                if (!string.IsNullOrWhiteSpace(downstreamBasePath))
                {
                    var cleanBasePath = downstreamBasePath.Trim('/');
                    var cleanRelativePath = relativePath.Trim('/');
                    relativePath = $"/{cleanBasePath}/{cleanRelativePath}";
                }

                var request = CreateHttpRequestMessage(relativePath, method, body, headers);
                
                // Enviar requisição
                _logger.LogInformation("Enviando requisição para: {ServiceName} - {Path}", 
                    serviceName, path);
                
                var response = await client.SendAsync(request);
                
                // Processar resposta
                var result = await ProcessResponseAsync(response);
                
                _logger.LogInformation("Resposta recebida: {StatusCode} - {ServiceName} - {Path}", 
                    result.StatusCode, serviceName, path);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao encaminhar requisição para: {ServiceName} - {Path}", 
                    serviceName, path);
                
                return new ProxyResponse
                {
                    StatusCode = 500,
                    Content = "Erro interno do servidor durante o encaminhamento da requisição"
                };
            }
        }

        private HttpRequestMessage CreateHttpRequestMessage(
            string path, HttpMethod method, object body, Dictionary<string, string> headers)
        {
            var request = new HttpRequestMessage(method, path);
            
            // Adicionar corpo da requisição se necessário
            if (body != null && (method != HttpMethod.Get && method != HttpMethod.Delete))
            {
                string content;
                
                if (body is string stringBody)
                {
                    content = stringBody;
                }
                else
                {
                    content = System.Text.Json.JsonSerializer.Serialize(body);
                }
                
                request.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }
            
            // Adicionar cabeçalhos personalizados
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        // Content-Type é gerenciado pelo Content
                        continue;
                    }

                    if (request.Headers.Contains(header.Key))
                    {
                        request.Headers.Remove(header.Key);
                    }

                    request.Headers.Add(header.Key, header.Value);
                }
            }
            
            return request;
        }

        private async Task<ProxyResponse> ProcessResponseAsync(HttpResponseMessage response)
        {
            var proxyResponse = new ProxyResponse
            {
                StatusCode = (int)response.StatusCode,
                ContentType = response.Content?.Headers?.ContentType?.MediaType
            };
            
            // Processar cabeçalhos
            foreach (var header in response.Headers)
            {
                // Ignorar cabeçalhos específicos do servidor que não devemos repassar
                if (header.Key.StartsWith("Server") || 
                    header.Key.StartsWith("Host") ||
                    header.Key.Equals("Transfer-Encoding"))
                {
                    continue;
                }
                
                proxyResponse.Headers[header.Key] = string.Join(", ", header.Value);
            }
            
            // Processar conteúdo
            if (response.Content != null)
            {
                proxyResponse.Content = await response.Content.ReadAsStringAsync();
            }
            
            return proxyResponse;
        }
    }
}