using System.Net.Http;

namespace APIGateway.ApplicationCore.Abstractions;

public interface IRouteConfigurationService
{
    bool IsServiceConfigured(string serviceName);
    bool IsRouteAllowed(string serviceName, string path, HttpMethod method);
    string GetServiceBaseUrl(string serviceName);
    int GetServiceTimeout(string serviceName);
    string GetDownstreamBasePath(string serviceName);
}