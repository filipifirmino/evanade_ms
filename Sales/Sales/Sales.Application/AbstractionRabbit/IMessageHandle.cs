namespace Sales.Application.AbstractionRabbit;

public interface IMessageHandle<in T> where T : class
{
    Task HandleAsync(T message, CancellationToken cancellationToken = default);
}