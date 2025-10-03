using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using APIGateway.ApplicationCore.Abstractions;
using APIGateway.ApplicationCore.Domain.Entities;

namespace APIGateway.Infra.RateLimiting;

public class InMemoryRateLimitService : IRateLimitService
{
    private readonly ConcurrentDictionary<string, List<DateTime>> _requests = new();
    private readonly IConfiguration _configuration;
    
    public InMemoryRateLimitService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<bool> AllowRequestAsync(string category, string clientId)
    {
        var policy = GetPolicyForEndpoint(category);
        var key = $"{clientId}:{category}";
        var now = DateTime.UtcNow;
        var windowStart = now.AddSeconds(-policy.WindowSeconds);
        
        var clientRequests = _requests.AddOrUpdate(key, 
            new List<DateTime> { now },
            (k, existing) =>
            {
                // Limpar requests antigas
                existing.RemoveAll(r => r < windowStart);
                existing.Add(now);
                return existing;
            });
        
        return clientRequests.Count <= policy.MaxRequests;
    }
    
    public async Task<RateLimitInfo> GetRateLimitInfoAsync(string clientId, string endpoint)
    {
        var policy = GetPolicyForEndpoint(endpoint);
        var key = $"{clientId}:{endpoint}";
        var now = DateTime.UtcNow;
        var windowStart = now.AddSeconds(-policy.WindowSeconds);
        
        if (!_requests.TryGetValue(key, out var requests))
            requests = new List<DateTime>();
        
        var currentRequests = requests.Count(r => r >= windowStart);
        var remaining = Math.Max(0, policy.MaxRequests - currentRequests);
        
        return new RateLimitInfo
        {
            Limit = policy.MaxRequests,
            Remaining = remaining,
            ResetTime = windowStart.AddSeconds(policy.WindowSeconds),
            Endpoint = endpoint,
            ClientId = clientId
        };
    }
    
    public RateLimitePolicy GetPolicyForEndpoint(string endpoint)
    {
        var section = _configuration.GetSection($"RateLimit:{endpoint}");
        
        if (section.Exists())
        {
            return new RateLimitePolicy
            {
                Endpoint = endpoint,
                MaxRequests = section.GetValue<int>("MaxRequests"),
                WindowSeconds = section.GetValue<int>("WindowSeconds")
            };
        }
        
        // Políticas padrão
        return endpoint switch
        {
            "auth" => new RateLimitePolicy { Endpoint = endpoint, MaxRequests = 5, WindowSeconds = 60 },
            "inventory" => new RateLimitePolicy { Endpoint = endpoint, MaxRequests = 100, WindowSeconds = 60 },
            "sales" => new RateLimitePolicy { Endpoint = endpoint, MaxRequests = 50, WindowSeconds = 60 },
            _ => new RateLimitePolicy { Endpoint = endpoint, MaxRequests = 30, WindowSeconds = 60 }
        };
    }
}