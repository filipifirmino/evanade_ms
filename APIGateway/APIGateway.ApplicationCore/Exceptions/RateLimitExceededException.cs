namespace APIGateway.ApplicationCore.Exceptions;

public class RateLimitExceededException : Exception
{
    public string ClientId { get; }
    public string Endpoint { get; }
    public int MaxRequests { get; }
    public int WindowSeconds { get; }

    public RateLimitExceededException(string clientId, string endpoint, int maxRequests, int windowSeconds) 
        : base($"Limite de requisições excedido para {endpoint}. Máximo: {maxRequests} em {windowSeconds} segundos")
    {
        ClientId = clientId;
        Endpoint = endpoint;
        MaxRequests = maxRequests;
        WindowSeconds = windowSeconds;
    }

    public RateLimitExceededException(string message) : base(message)
    {
    }

    public RateLimitExceededException(string message, Exception innerException) : base(message, innerException)
    {
    }
}