using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;

namespace Bizcord.MessageClient;

public sealed class EasyNetQMessageClient(IBus bus) : IMessageClient
{
    public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
        => bus.PubSub.PublishAsync(message, cancellationToken);

    public async Task<IAsyncDisposable> SubscribeAsync<T>(
        string subscriptionId,
        Func<T, CancellationToken, Task> handler,
        CancellationToken cancellationToken = default) where T : class
    {
        var subscription = await bus.PubSub.SubscribeAsync(subscriptionId, handler, _ => { }, cancellationToken);
        return subscription;
    }
}

public static class MessageClientRegistration
{
    public static IServiceCollection AddMessageClient(this IServiceCollection services, string connectionString)
    {
        services.AddEasyNetQ(connectionString).UseSystemTextJson();
        services.AddSingleton<IMessageClient, EasyNetQMessageClient>();
        return services;
    }
}
