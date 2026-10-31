using Bizcord.MessageClient;
using Bizcord.Messaging.Api.Domain;
using Bizcord.Shared.Contracts.Events;
using Bizcord.Shared.Contracts.Models;
using Bizcord.Shared.Contracts.Requests;

namespace Bizcord.Messaging.Api.Application;

public sealed class MessageService(IMessageRepository repository, IMessageClient messageClient)
{
    public async Task<MessageDto> CreateAsync(CreateMessageRequest request, CancellationToken cancellationToken)
    {
        var message = Message.Create(request.ChannelId, request.AuthorId, request.Content);
        await repository.SaveAsync(message, cancellationToken);

        try
        {
            await messageClient.PublishAsync(new MessagePostedEvent
            {
                MessageId = message.Id,
                ChannelId = message.ChannelId,
                AuthorId = message.AuthorId,
                Content = message.Content.Value,
                PostedAt = message.PostedAt
            }, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new MessagePublicationException(message.Id, exception);
        }

        return ToDto(message);
    }

    public async Task<MessageDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var message = await repository.GetAsync(id, cancellationToken);
        return message is null ? null : ToDto(message);
    }

    public async Task<IReadOnlyList<MessageDto>> ListAsync(Guid? channelId, CancellationToken cancellationToken)
        => (await repository.ListAsync(channelId, cancellationToken)).Select(ToDto).ToArray();

    public async Task<MessageDto?> UpdateAsync(Guid id, UpdateMessageRequest request, CancellationToken cancellationToken)
    {
        var message = await repository.GetAsync(id, cancellationToken);
        if (message is null) return null;
        message.UpdateContent(request.Content);
        return await repository.UpdateAsync(message, cancellationToken) ? ToDto(message) : null;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        => repository.DeleteAsync(id, cancellationToken);

    private static MessageDto ToDto(Message message)
        => new(message.Id, message.ChannelId, message.AuthorId, message.Content.Value, message.PostedAt);
}
