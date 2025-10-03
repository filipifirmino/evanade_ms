namespace APIGateway.ApplicationCore.Domain.Entities;

public class RateLimitePolicy
{
    public string Endpoint { get; set; }
    public int MaxRequests { get; set; }
    public int WindowSeconds { get; set; }
        
    public bool IsWithinLimit(int currentRequests) => 
        currentRequests <= MaxRequests;
}