using Sales.BFF.ApplicationCore.DTOs;

namespace Sales.BFF.ApplicationCore.Abstractions;

public interface IProxyService
{
    Task<ProxyResponse> ForwardRequestAsync(string serviceName, 
        string path, HttpMethod method, object body = null, 
        Dictionary<string, string> headers = null);
}