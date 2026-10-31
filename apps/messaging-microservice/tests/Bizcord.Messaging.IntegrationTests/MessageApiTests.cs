using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bizcord.Shared.Contracts.Events;
using Bizcord.Shared.Contracts.Models;
using Bizcord.Shared.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Bizcord.Messaging.IntegrationTests;

public sealed class MessageApiTests
{
    [Fact]
    public async Task Post_Returns201WithLocation_AndPersistsMessage()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new MessagingApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateMessageRequest(Guid.NewGuid(), Guid.NewGuid(), "Hello world");

        var response = await client.PostAsJsonAsync("/api/messages", request, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var message = Assert.IsType<MessageDto>(await response.Content.ReadFromJsonAsync<MessageDto>(ct));
        Assert.NotEqual(Guid.Empty, message.MessageId);
        Assert.EndsWith($"/api/messages/{message.MessageId}", response.Headers.Location!.ToString());
        var stored = await client.GetFromJsonAsync<MessageDto>(response.Headers.Location, ct);
        Assert.Equal(message, stored);
    }

    [Fact]
    public async Task Post_PublishedEvent_MeetsEveryFieldOfTheProviderContract()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new MessagingApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateMessageRequest(Guid.NewGuid(), Guid.NewGuid(), "Contract example");
        var before = DateTime.UtcNow;

        var response = await client.PostAsJsonAsync("/api/messages", request, ct);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = Assert.IsType<MessageDto>(await response.Content.ReadFromJsonAsync<MessageDto>(ct));
        var published = Assert.Single(factory.Messages.Published.OfType<MessagePostedEvent>());
        Assert.NotEqual(Guid.Empty, published.MessageId);
        Assert.Equal(body.MessageId, published.MessageId);
        Assert.Equal(request.ChannelId, published.ChannelId);
        Assert.Equal(request.AuthorId, published.AuthorId);
        Assert.Equal(request.Content, published.Content);
        Assert.Equal(body.PostedAt, published.PostedAt);
        Assert.Equal(DateTimeKind.Utc, published.PostedAt.Kind);
        Assert.InRange(published.PostedAt, before, DateTime.UtcNow);
    }

    [Fact]
    public async Task Crud_CanFilterUpdateAndDelete_WithoutRepostingEvents()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new MessagingApiFactory();
        using var client = factory.CreateClient();
        var channelId = Guid.NewGuid();
        var created = await client.PostAsJsonAsync("/api/messages", new CreateMessageRequest(channelId, Guid.NewGuid(), "Original"), ct);
        var original = Assert.IsType<MessageDto>(await created.Content.ReadFromJsonAsync<MessageDto>(ct));
        await client.PostAsJsonAsync("/api/messages", new CreateMessageRequest(Guid.NewGuid(), Guid.NewGuid(), "Other channel"), ct);
        var url = $"/api/messages/{original.MessageId}";

        var updated = await client.PutAsJsonAsync(url, new UpdateMessageRequest("Edited"), ct);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var edited = Assert.IsType<MessageDto>(await updated.Content.ReadFromJsonAsync<MessageDto>(ct));
        Assert.Equal(original with { Content = "Edited" }, edited);
        var filtered = await client.GetFromJsonAsync<MessageDto[]>($"/api/messages?channelId={channelId}", ct);
        Assert.Equal(edited, Assert.Single(Assert.IsType<MessageDto[]>(filtered)));
        Assert.Equal(2, factory.Messages.Published.Count);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync(url, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(url, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync(url, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync(url, new UpdateMessageRequest("Missing"), ct)).StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Post_InvalidContent_Returns400WithoutSavingOrPublishing(string content)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new MessagingApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/messages", new CreateMessageRequest(Guid.NewGuid(), Guid.NewGuid(), content), ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        Assert.Empty(factory.Messages.Published);
        Assert.Empty(Assert.IsType<MessageDto[]>(await client.GetFromJsonAsync<MessageDto[]>("/api/messages", ct)));
    }

    [Fact]
    public async Task Post_EmptyReference_Returns400WithoutPublishing()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new MessagingApiFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/messages", new CreateMessageRequest(Guid.Empty, Guid.NewGuid(), "Hello"), ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(factory.Messages.Published);
    }

    [Fact]
    public async Task Put_InvalidContent_PreservesStoredMessage()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new MessagingApiFactory();
        using var client = factory.CreateClient();
        var created = await client.PostAsJsonAsync("/api/messages", new CreateMessageRequest(Guid.NewGuid(), Guid.NewGuid(), "Keep me"), ct);
        var url = created.Headers.Location!;
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync(url, new UpdateMessageRequest(" "), ct)).StatusCode);
        Assert.Equal("Keep me", (await client.GetFromJsonAsync<MessageDto>(url, ct))!.Content);
    }

    [Fact]
    public async Task Post_BrokerFailure_Returns503WithRecoverableMessageId()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new MessagingApiFactory();
        factory.Messages.PublishFailure = new IOException("Broker unavailable");
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/messages", new CreateMessageRequest(Guid.NewGuid(), Guid.NewGuid(), "Saved first"), ct);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(await response.Content.ReadFromJsonAsync<ProblemDetails>(ct));
        var messageId = Assert.IsType<JsonElement>(problem.Extensions["messageId"]).GetGuid();
        Assert.True(Assert.IsType<JsonElement>(problem.Extensions["persisted"]).GetBoolean());
        Assert.Equal("Saved first", (await client.GetFromJsonAsync<MessageDto>($"/api/messages/{messageId}", ct))!.Content);
    }

    [Fact]
    public async Task MalformedJson_Returns400_AndHealthReturns200()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var factory = new MessagingApiFactory();
        using var client = factory.CreateClient();
        using var content = new StringContent("{not-json", System.Text.Encoding.UTF8, "application/json");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/api/messages", content, ct)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health", ct)).StatusCode);
        Assert.Empty(factory.Messages.Published);
    }
}
