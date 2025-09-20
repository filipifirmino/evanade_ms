namespace Sales.BFF.ApplicationCore.Domain.Entities;

public class ServiceRoute
{
    public string ServiceName { get; set; }
    public string BaseUrl { get; set; }
    public string Template { get; set; }
    public List<string> Methods { get; set; } = new();
    public int TimeoutSeconds { get; set; } = 30;
        
    public bool SupportsMethod(string method) => 
        Methods.Contains(method.ToUpperInvariant());
}