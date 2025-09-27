namespace Sales.Application.AbstractionRabbit;

/// <summary>
/// Interface para produtor genérico de eventos que implementam IEventWithQueueConfiguration
/// </summary>
public interface IGenericEventProducer
{
    /// <summary>
    /// Publica um evento que implementa IEventWithQueueConfiguration
    /// </summary>
    /// <typeparam name="T">Tipo do evento que deve implementar IEventWithQueueConfiguration</typeparam>
    /// <param name="eventData">Dados do evento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Task representando a operação assíncrona</returns>
    Task PublishEventAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : class;
}
