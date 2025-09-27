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
                // Verificar se a rota corresponde ao template
                if (IsRouteMatch(route.Template, path) && 
                    route.Methods.Contains(method.Method))
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
            // Remover o prefixo /api/ caso exista
            if (path.StartsWith("/api/"))
            {
                path = path.Substring(5);
            }

            // Dividir a rota e o template em segmentos
            var routeSegments = path.Split('/');
            var templateSegments = template.Split('/');

            if (routeSegments.Length != templateSegments.Length)
            {
                return false;
            }

            for (int i = 0; i < templateSegments.Length; i++)
            {
                var templateSegment = templateSegments[i];
                var routeSegment = routeSegments[i];

                // Se o segmento é um parâmetro (entre chaves {param})
                if (templateSegment.StartsWith("{") && templateSegment.EndsWith("}"))
                {
                    continue; // Considerar correspondência para qualquer valor
                }
                else if (templateSegment != routeSegment)
                {
                    return false; // Não corresponde
                }
            }

            return true;
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