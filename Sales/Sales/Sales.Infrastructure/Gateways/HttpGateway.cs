using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sales.Application.AbstractionsGateways;

namespace Sales.Infrastructure.Gateways;

public class HttpGateway(IConfiguration configuration, HttpClient httpClient, ILogger<HttpGateway> logger) : IHttpGateway
{
    public async Task<int> GetProductStockQuantity(Guid productId, string authorizationToken = null)
    {
        try
        {
            var baseUrl = configuration.GetSection("ProductUrl").Value;
            
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("id", productId.ToString());
            
            // Adicionar token de autorização se fornecido
            if (!string.IsNullOrEmpty(authorizationToken))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", authorizationToken);
            }
            
            var response = await httpClient.GetAsync($"{baseUrl}quantity-available-product-by-id");
            response.EnsureSuccessStatusCode();
            
            var responseBody = await response.Content.ReadAsStringAsync();
            
            return int.Parse(responseBody);
            
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