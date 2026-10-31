using System.Net;
using System.Net.Http.Json;
using Bizcord.MessageClient;
using Bizcord.Shared.Contracts.Events;
using Bizcord.Shared.Contracts.Models;
using Bizcord.Shared.Contracts.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Bizcord.Messaging.IntegrationTests;

public sealed class BrokerContractTests
{
    [Fact]
    [Trait("Category", "BrokerIntegration")]
    public async Task PostMessage_PersistsAndPublishesExactContract_ThroughRealRabbitMq()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new MessagingApiFactory(useRealBroker: true);
        using var http = factory.CreateClient();
        var messageClient = factory.Services.GetRequiredService<IMessageClient>();
        var channelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var received = new TaskCompletionSource<MessagePostedEvent>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var subscription = await messageClient.SubscribeAsync<MessagePostedEvent>(
            "bizcord-assignment-verification", (message, _) =>
            {
                if (message.ChannelId == channelId) received.TrySetResult(message);
                return Task.CompletedTask;
            }, ct);
        var before = DateTime.UtcNow;

        var response = await http.PostAsJsonAsync("/api/messages", new CreateMessageRequest(channelId, authorId, "Real broker contract"), ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = Assert.IsType<MessageDto>(await response.Content.ReadFromJsonAsync<MessageDto>(ct));
        var published = await received.Task.WaitAsync(TimeSpan.FromSeconds(15), ct);
        Assert.NotEqual(Guid.Empty, published.MessageId);
        Assert.Equal(body.MessageId, published.MessageId);
        Assert.Equal(channelId, published.ChannelId);
        Assert.Equal(authorId, published.AuthorId);
        Assert.Equal("Real broker contract", published.Content);
        Assert.Equal(body.PostedAt, published.PostedAt);
        Assert.Equal(DateTimeKind.Utc, published.PostedAt.Kind);
        Assert.InRange(published.PostedAt, before, DateTime.UtcNow);
        Assert.Equal(body, await http.GetFromJsonAsync<MessageDto>(response.Headers.Location, ct));
    }
}
