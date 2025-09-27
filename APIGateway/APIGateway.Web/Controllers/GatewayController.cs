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
        
        // SALES
        [HttpPost("sales/orders")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest orderRequest)
        {
            return await HandleProxyRequest("SalesService", "sales/orders", HttpMethod.Post);
        }

        [HttpGet("sales/orders/{id}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            return await HandleProxyRequest("SalesService", $"sales/orders/{id}", HttpMethod.Get);
        }

        [HttpGet("sales/orders")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> ListOrders()
        {
            // Query string é preservada pelo HttpContext, montada no HandleProxyRequest
            return await HandleProxyRequest("SalesService", "sales/orders", HttpMethod.Get);
        }

        // STOCK / INVENTORY
        [HttpPost("inventory/products")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct()
        {
            return await HandleProxyRequest("InventoryService", "inventory/products", HttpMethod.Post);
        }

        [HttpGet("inventory/products")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> ListProducts()
        {
            return await HandleProxyRequest("InventoryService", "inventory/products", HttpMethod.Get);
        }

        [HttpGet("inventory/products/{sku}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> GetProductBySku(string sku)
        {
            return await HandleProxyRequest("InventoryService", $"inventory/products/{sku}", HttpMethod.Get);
        }

        [HttpPut("inventory/products/{sku}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(string sku)
        {
            return await HandleProxyRequest("InventoryService", $"inventory/products/{sku}", HttpMethod.Put);
        }
        
        private async Task<IActionResult> HandleProxyRequest(string serviceName, 
            string path, HttpMethod method)
        {
            try
            {
                // Montar fullPath com query string preservada
                var basePath = $"/api/{path}";
                var queryString = Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty;
                var fullPath = string.Concat(basePath, queryString);
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