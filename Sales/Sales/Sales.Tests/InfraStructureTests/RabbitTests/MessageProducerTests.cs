using AutoBogus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using Sales.Application.Events;
using Sales.Application.Settings;
using Sales.Infrastructure.Rabbit;
using Sales.Infrastructure.Rabbit.Producers;

namespace Sales.Tests.InfraStructureTests.RabbitTests;

public class MessageProducerTests
{
    private readonly Mock<IRabbitMqConnection> _connectionMock;
    private readonly Mock<ILogger<MessageProducer>> _loggerMock;
    private readonly Mock<IOptions<RabbitMq>> _settingsMock;
    private readonly Mock<IModel> _channelMock;
    private readonly MessageProducer _producer;

    public MessageProducerTests()
    {
        _connectionMock = new Mock<IRabbitMqConnection>();
        _loggerMock = new Mock<ILogger<MessageProducer>>();
        _settingsMock = new Mock<IOptions<RabbitMq>>();
        _channelMock = new Mock<IModel>();

        var rabbitSettings = new RabbitMq
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            VirtualHost = "/",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };

        _settingsMock.Setup(x => x.Value).Returns(rabbitSettings);
        _connectionMock.Setup(x => x.CreateChannel()).Returns(_channelMock.Object);

        _producer = new MessageProducer(_connectionMock.Object, _loggerMock.Object, _settingsMock.Object);
    }

    [Fact]
    public async Task PublishAsync_WithQueueNameOnly_ShouldPublishToQueue()
    {
        var orderCreated = AutoFaker.Generate<OrderCreated>();
        var queueName = "test-queue";

        _channelMock.Setup(x => x.ExchangeDeclare(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        _channelMock.Setup(x => x.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        _channelMock.Setup(x => x.QueueBind(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()));
        _channelMock.Setup(x => x.CreateBasicProperties()).Returns(new Mock<IBasicProperties>().Object);
        _channelMock.Setup(x => x.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>()));

        await _producer.PublishAsync(orderCreated, queueName);

        _connectionMock.Verify(x => x.CreateChannel(), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        var orderCreated = AutoFaker.Generate<OrderCreated>();
        var cancellationToken = new CancellationToken();

        _channelMock.Setup(x => x.ExchangeDeclare(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        _channelMock.Setup(x => x.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        _channelMock.Setup(x => x.QueueBind(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()));
        _channelMock.Setup(x => x.CreateBasicProperties()).Returns(new Mock<IBasicProperties>().Object);
        _channelMock.Setup(x => x.BasicPublish(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicProperties>(), It.IsAny<ReadOnlyMemory<byte>>()));

        await _producer.PublishAsync(orderCreated, "test-queue", cancellationToken);

        _connectionMock.Verify(x => x.CreateChannel(), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WhenChannelThrowsException_ShouldLogErrorAndRethrow()
    {
        var orderCreated = AutoFaker.Generate<OrderCreated>();
        var exception = new Exception("Channel error");

        _channelMock.Setup(x => x.ExchangeDeclare(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()))
            .Throws(exception);

        await Assert.ThrowsAsync<Exception>(() => _producer.PublishAsync(orderCreated, "test-queue"));

        _connectionMock.Verify(x => x.CreateChannel(), Times.Once);
    }
}