namespace APIGateway.ApplicationCore.Domain.Entities;

public class RateLimitInfo
{
    public int Limit { get; set; }
    public int Remaining { get; set; }
    public DateTime ResetTime { get; set; }
    public string Endpoint { get; set; }
    public string ClientId { get; set; }
}

