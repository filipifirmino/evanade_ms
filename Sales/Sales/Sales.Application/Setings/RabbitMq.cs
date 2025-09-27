namespace Sales.Application.Setings;

public class RabbitMq
{
    public const string SectionName = "RabbitMQ";
        
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(10);
    public List<QueueConfiguration> Queues { get; set; } = new();
}

public class QueueConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
    public bool Exclusive { get; set; } = false;
    public Dictionary<string, object> Arguments { get; set; } = new();
}