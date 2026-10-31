using System.Collections.Concurrent;
using Bizcord.MessageClient;

namespace Bizcord.Messaging.IntegrationTests;

public sealed class FakeMessageClient : IMessageClient
{
    public ConcurrentQueue<object> Published { get; } = new();
    public Exception? PublishFailure { get; set; }

    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (PublishFailure is not null) return Task.FromException(PublishFailure);
        Published.Enqueue(message);
        return Task.CompletedTask;
    }

    public Task<IAsyncDisposable> SubscribeAsync<T>(string subscriptionId, Func<T, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default) where T : class
        => throw new NotSupportedException("This fake captures publication only. Subscriptions are verified against real RabbitMQ.");
}
