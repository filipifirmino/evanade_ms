namespace Sales.BFF.ApplicationCore.DTOs;

public class ProxyResponse
{
    public int StatusCode { get; set; }
    public string Content { get; set; }
    public string ContentType { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
}