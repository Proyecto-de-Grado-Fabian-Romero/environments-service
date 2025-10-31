namespace EnvironmentsService.Src.Application.Interfaces;

public interface IMessageBus
{
    Task PublishAsync<T>(T message, string queueName);

    Task<TResponse> RequestAsync<TRequest, TResponse>(
        TRequest request,
        string requestQueue,
        string replyQueue
    );
}
