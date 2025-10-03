using Microsoft.Extensions.Configuration;
using APIGateway.ApplicationCore.Abstractions;
using APIGateway.ApplicationCore.Domain.Entities;

namespace APIGateway.Infra.Configuration
{
    public class RouteConfigurationService : IRouteConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, List<ServiceRoute>> _serviceRoutes = new();

        public RouteConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
            InitializeRoutes();
        }

        public bool IsServiceConfigured(string serviceName)
        {
            return _configuration.GetSection($"Services:{serviceName}").Exists() &&
                   !string.IsNullOrEmpty(_configuration[$"Services:{serviceName}:BaseUrl"]);
        }

        public bool IsRouteAllowed(string serviceName, string path, HttpMethod method)
        {
            if (!_serviceRoutes.TryGetValue(serviceName, out var routes))
            {
                return false;
            }

            foreach (var route in routes)
            {
                bool matches = IsRouteMatch(route.Template, path);
                bool methodAllowed = route.Methods.Contains(method.Method);
                
                if (matches && methodAllowed)
                {
                    return true;
                }
            }

            return false;
        }

        public string GetServiceBaseUrl(string serviceName)
        {
            return _configuration[$"Services:{serviceName}:BaseUrl"];
        }

        public int GetServiceTimeout(string serviceName)
        {
            return _configuration.GetValue<int>($"Services:{serviceName}:TimeoutSeconds", 30);
        }

        public string GetDownstreamBasePath(string serviceName)
        {
            return _configuration[$"Services:{serviceName}:DownstreamBasePath"] ?? string.Empty;
        }

        private bool IsRouteMatch(string template, string path)
        {
            // Normalizar o path removendo barras no início
            path = path.TrimStart('/');
            template = template.TrimStart('/');

            // Fazer comparação direta simples
            return string.Equals(template, path, StringComparison.OrdinalIgnoreCase);
        }

        private void InitializeRoutes()
        {
            var servicesSection = _configuration.GetSection("Services");
            foreach (var serviceSection in servicesSection.GetChildren())
            {
                var serviceName = serviceSection.Key;
                var routes = new List<ServiceRoute>();

                var routesSection = serviceSection.GetSection("Routes");
                foreach (var routeSection in routesSection.GetChildren())
                {
                    var template = routeSection["Template"];
                    var methods = routeSection.GetSection("Methods")
                        .Get<string[]>()?.ToList() ?? new List<string> { "GET" };

                    routes.Add(new ServiceRoute
                    {
                        Template = template,
                        Methods = methods
                    });
                }

                _serviceRoutes[serviceName] = routes;
            }
        }
    }
}