using APIGateway.ApplicationCore.Domain.Entities;

namespace APIGateway.ApplicationCore.Abstractions;

public interface IRateLimitService
{
    Task<bool> AllowRequestAsync(string category, string clientId);
    Task<RateLimitInfo> GetRateLimitInfoAsync(string clientId, string endpoint);
    RateLimitePolicy GetPolicyForEndpoint(string endpoint);
}