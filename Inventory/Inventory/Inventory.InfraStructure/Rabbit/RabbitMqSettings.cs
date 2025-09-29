using System;

namespace Inventory.InfraStructure.Rabbit;

public class RabbitMqSettings
{
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public int Port { get; set; } = 5672;
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(10);
}