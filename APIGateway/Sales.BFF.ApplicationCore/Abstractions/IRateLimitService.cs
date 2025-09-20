namespace Sales.BFF.ApplicationCore.Abstractions;

public interface IRateLimitService
{
    Task<bool> IsRateLimitedAsync(string clientId, string endpoint);
    Task<RateLimitInfo> GetRateLimitInfoAsync(string clientId, string endpoint);
    RateLimitPolicy GetPolicyForEndpoint(string endpoint);
}