using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sales.Application.AbstractionsGateways;

namespace Sales.Infrastructure.Gateways;

public class HttpGateway(IConfiguration configuration, HttpClient httpClient, ILogger<HttpGateway> logger) : IHttpGateway
{
    public async Task<int> GetProductStockQuantity(Guid productId)
    {
        try
        {
            var baseUrl = configuration.GetSection("ProductUrl").Value;
            
            // Limpar headers anteriores e adicionar o ID do produto
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("id", productId.ToString());
            
            var response = await httpClient.GetAsync($"{baseUrl}product-by-id");
            response.EnsureSuccessStatusCode();
            
            var responseBody = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(responseBody);
            
            if (doc.RootElement.TryGetProperty("stockQuantity", out JsonElement stockQuantityElement))
            {
                return stockQuantityElement.GetInt32();
            }
            
            logger.LogError("Response does not contain 'stockQuantity' property for product {ProductId}", productId);
            throw new InvalidOperationException($"Product {productId} stock information not found");
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error fetching stock for product {ProductId}", productId);
            throw new InvalidOperationException($"Failed to retrieve stock information for product {productId}", ex);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Invalid JSON response for product {ProductId}", productId);
            throw new InvalidOperationException($"Invalid response format for product {productId}", ex);
        }
    }
}