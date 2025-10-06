using AutoBogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sales.Application.AbstractionRabbit;
using Sales.Application.Events;
using Sales.Infrastructure.Rabbit;
using Sales.Infrastructure.Rabbit.Consumers;

namespace Sales.Tests.InfraStructureTests.RabbitTests;

public class MessageConsumerTests
{
    private readonly Mock<IRabbitMqConnection> _connectionMock;
    private readonly Mock<ILogger<MessageConsumer<OrderConfirmed>>> _loggerMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly Mock<IModel> _channelMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IMessageHandle<OrderConfirmed>> _messageHandlerMock;
    private readonly MessageConsumer<OrderConfirmed> _consumer;

    public MessageConsumerTests()
    {
        _connectionMock = new Mock<IRabbitMqConnection>();
        _loggerMock = new Mock<ILogger<MessageConsumer<OrderConfirmed>>>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        _channelMock = new Mock<IModel>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _messageHandlerMock = new Mock<IMessageHandle<OrderConfirmed>>();

        _connectionMock.Setup(x => x.CreateChannel()).Returns(_channelMock.Object);
        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(x => x.GetService<IMessageHandle<OrderConfirmed>>()).Returns(_messageHandlerMock.Object);

        _consumer = new MessageConsumer<OrderConfirmed>(
            _connectionMock.Object,
            _loggerMock.Object,
            _serviceScopeFactoryMock.Object);
    }

    [Fact]
    public async Task StartAsync_ShouldSetRunningStateToTrue()
    {
        await _consumer.StartAsync();

        Assert.True(_consumer.IsRunning);
        _connectionMock.Verify(x => x.CreateChannel(), Times.Never);
    }

    [Fact]
    public async Task StartAsync_WhenAlreadyRunning_ShouldLogWarning()
    {
        await _consumer.StartAsync();
        await _consumer.StartAsync();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Consumer já está rodando")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StopAsync_ShouldSetRunningStateToFalse()
    {
        await _consumer.StartAsync();
        await _consumer.StopAsync();

        Assert.False(_consumer.IsRunning);
    }

    [Fact]
    public async Task StopAsync_WhenNotRunning_ShouldLogWarning()
    {
        await _consumer.StopAsync();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Consumer não está rodando")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartConsumingAsync_WithQueueNameOnly_ShouldStartConsuming()
    {
        var queueName = "test-queue";

        _channelMock.Setup(x => x.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        _channelMock.Setup(x => x.BasicQos(It.IsAny<uint>(), It.IsAny<ushort>(), It.IsAny<bool>()));
        _channelMock.Setup(x => x.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicConsumer>()));

        await _consumer.StartConsumingAsync(queueName);

        Assert.True(_consumer.IsRunning);
        _connectionMock.Verify(x => x.CreateChannel(), Times.Once);
    }

    [Fact]
    public async Task StartConsumingAsync_WhenChannelThrowsException_ShouldLogErrorAndRethrow()
    {
        var queueName = "test-queue";
        var exception = new Exception("Channel error");

        _channelMock.Setup(x => x.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()))
            .Throws(exception);

        await Assert.ThrowsAsync<Exception>(() => _consumer.StartConsumingAsync(queueName));

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Erro ao iniciar consumer")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task StartConsumingAsync_WhenNotRunning_ShouldStartFirst()
    {
        var queueName = "test-queue";

        _channelMock.Setup(x => x.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()));
        _channelMock.Setup(x => x.BasicQos(It.IsAny<uint>(), It.IsAny<ushort>(), It.IsAny<bool>()));
        _channelMock.Setup(x => x.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<IBasicConsumer>()));

        await _consumer.StartConsumingAsync(queueName);

        Assert.True(_consumer.IsRunning);
    }
}