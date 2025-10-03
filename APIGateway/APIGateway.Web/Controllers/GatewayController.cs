using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using APIGateway.ApplicationCore.Abstractions;
using APIGateway.ApplicationCore.DTOs;

namespace APIGateway.Web.Controllers;

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
        
        [HttpPost("sales/orders")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest orderRequest)
        {
            return await HandleProxyRequest("SalesService", "create-order", HttpMethod.Post);
        }

        [HttpGet("sales/orders/{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            return await HandleProxyRequest("SalesService", "get-by-id", HttpMethod.Get, id);
        }

        [HttpGet("sales/orders")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> ListOrders()
        {
            return await HandleProxyRequest("SalesService", "all-order", HttpMethod.Get);
        }

        [HttpPut("sales/orders/{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> UpdateOrder(string id, [FromBody] OrderRequest orderRequest)
        {
            return await HandleProxyRequest("SalesService", "update-order", HttpMethod.Put, id);
        }

        [HttpDelete("sales/orders")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> DeleteOrder([FromBody] OrderRequest orderRequest)
        {
            return await HandleProxyRequest("SalesService", "remove-order", HttpMethod.Delete);
        }

      
        [HttpPost("inventory/products")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductRequest productRequest)
        {
            return await HandleProxyRequest("InventoryService", "create-product", HttpMethod.Post);
        }

        [HttpGet("inventory/products")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> ListProducts()
        {
            return await HandleProxyRequest("InventoryService", "all-products", HttpMethod.Get);
        }

        [HttpGet("inventory/products/{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetProductById(string id)
        {
            return await HandleProxyRequest("InventoryService", "product-by-id", HttpMethod.Get, id);
        }

        [HttpGet("inventory/products/{id}/quantity")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetProductQuantity(string id)
        {
            return await HandleProxyRequest("InventoryService", "quantity-available-product-by-id", HttpMethod.Get, id);
        }

        [HttpPut("inventory/products")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductRequest productRequest)
        {
            return await HandleProxyRequest("InventoryService", "update-product", HttpMethod.Put);
        }

        [HttpDelete("inventory/products")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct([FromBody] ProductRequest productRequest)
        {
            return await HandleProxyRequest("InventoryService", "remove-product", HttpMethod.Delete);
        }
        
        private async Task<IActionResult> HandleProxyRequest(string serviceName, 
            string path, HttpMethod method, string parameter = null)
        {
            try
            {
                var fullPath = $"/{path}{Request.QueryString}";
                var headers = ExtractHeaders();
                
                // Adicionar parâmetro como header se necessário
                if (!string.IsNullOrEmpty(parameter) && Guid.TryParse(parameter, out _))
                {
                    headers["id"] = parameter;
                }
                
                var body = method != HttpMethod.Get ? await ExtractBodyAsync() : null;
                
                var response = await _proxyService.ForwardRequestAsync(
                    serviceName, fullPath, method, body, headers);
                
                return HandleResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling proxy request for {serviceName}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        
        private IActionResult HandleResponse(ProxyResponse response)
        {
            if (string.IsNullOrEmpty(response.Content))
            {
                return StatusCode(response.StatusCode);
            }
            
            // Tentar deserializar como JSON
            if (response.Content.TrimStart().StartsWith("{") || response.Content.TrimStart().StartsWith("["))
            {
                try
                {
                    return StatusCode(response.StatusCode, 
                        System.Text.Json.JsonSerializer.Deserialize<object>(response.Content));
                }
                catch (System.Text.Json.JsonException)
                {
                    // Fallback para texto simples
                }
            }
            
            return StatusCode(response.StatusCode, new { message = response.Content });
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