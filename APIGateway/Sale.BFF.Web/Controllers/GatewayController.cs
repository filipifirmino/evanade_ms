using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sales.BFF.ApplicationCore.Abstractions;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class GatewayController: ControllerBase
{
    private readonly IProxyService _proxyService;
        private readonly ILogger<GatewayController> _logger;
        
        public GatewayController(IProxyService proxyService, ILogger<GatewayController> logger)
        {
            _proxyService = proxyService;
            _logger = logger;
        }
        
        [HttpGet("{serviceName}/{*path}")]
        public async Task<IActionResult> ProxyGet(string serviceName, string path)
        {
            return await HandleProxyRequest(serviceName, path, HttpMethod.Get);
        }
        
        [HttpPost("{serviceName}/{*path}")]
        public async Task<IActionResult> ProxyPost(string serviceName, string path)
        {
            return await HandleProxyRequest(serviceName, path, HttpMethod.Post);
        }
        
        [HttpPut("{serviceName}/{*path}")]
        public async Task<IActionResult> ProxyPut(string serviceName, string path)
        {
            return await HandleProxyRequest(serviceName, path, HttpMethod.Put);
        }
        
        [HttpDelete("{serviceName}/{*path}")]
        public async Task<IActionResult> ProxyDelete(string serviceName, string path)
        {
            return await HandleProxyRequest(serviceName, path, HttpMethod.Delete);
        }
        
        private async Task<IActionResult> HandleProxyRequest(string serviceName, 
            string path, HttpMethod method)
        {
            try
            {
                var fullPath = $"/api/{serviceName}/{path}";
                var headers = ExtractHeaders();
                var body = method != HttpMethod.Get ? await ExtractBodyAsync() : null;
                
                _logger.LogInformation($"Proxying {method} request to {serviceName}: {fullPath}");
                
                var response = await _proxyService.ForwardRequestAsync(
                    serviceName, fullPath, method, body, headers);
                
                return StatusCode(response.StatusCode, 
                    string.IsNullOrEmpty(response.Content) ? null : 
                    System.Text.Json.JsonSerializer.Deserialize<object>(response.Content));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling proxy request for {serviceName}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        
        private Dictionary<string, string> ExtractHeaders()
        {
            var headers = new Dictionary<string, string>();
            
            if (Request.Headers.TryGetValue("Authorization", out var auth))
                headers["Authorization"] = auth.FirstOrDefault();
                
            if (Request.Headers.TryGetValue("Content-Type", out var contentType))
                headers["Content-Type"] = contentType.FirstOrDefault();
                
            return headers;
        }
        
        private async Task<object> ExtractBodyAsync()
        {
            if (Request.ContentLength > 0)
            {
                Request.EnableBuffering();
                Request.Body.Position = 0;
                
                var body = await new StreamReader(Request.Body).ReadToEndAsync();
                Request.Body.Position = 0;
                
                return body;
            }
            
            return null;
        }
}