using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Polly.RateLimit;
using Sales.BFF.ApplicationCore.Abstractions;

namespace Sales.BFF.Infra.RateLimiting;

public class InMemoryRateLimitService : IRateLimitService
{
    private readonly ConcurrentDictionary<string, List<DateTime>> _requests = new();
        private readonly IConfiguration _configuration;
        
        public InMemoryRateLimitService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public async Task<bool> IsRateLimitedAsync(string clientId, string endpoint)
        {
            var policy = GetPolicyForEndpoint(endpoint);
            var key = $"{clientId}:{endpoint}";
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
            
            return clientRequests.Count > policy.MaxRequests;
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
                ResetTime = windowStart.AddSeconds(policy.WindowSeconds)
            };
        }
        
        public RateLimitPolicy GetPolicyForEndpoint(string endpoint)
        {
            var section = _configuration.GetSection($"RateLimit:{endpoint}");
            
            if (section.Exists())
            {
                return new RateLimitPolicy
                {
                    Endpoint = endpoint,
                    MaxRequests = section.GetValue<int>("MaxRequests"),
                    WindowSeconds = section.GetValue<int>("WindowSeconds")
                };
            }
            
            // Políticas padrão
            return endpoint switch
            {
                "auth" => new RateLimitPolicy { Endpoint = endpoint, MaxRequests = 5, WindowSeconds = 60 },
                "inventory" => new RateLimitPolicy { Endpoint = endpoint, MaxRequests = 100, WindowSeconds = 60 },
                "sales" => new RateLimitPolicy { Endpoint = endpoint, MaxRequests = 50, WindowSeconds = 60 },
                _ => new RateLimitPolicy { Endpoint = endpoint, MaxRequests = 30, WindowSeconds = 60 }
            };
}