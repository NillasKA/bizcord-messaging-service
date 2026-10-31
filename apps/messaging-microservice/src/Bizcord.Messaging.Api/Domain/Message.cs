namespace Bizcord.Messaging.Api.Domain;

public sealed class Message
{
    public Guid Id { get; }
    public Guid ChannelId { get; }
    public Guid AuthorId { get; }
    public MessageContent Content { get; private set; }
    public DateTime PostedAt { get; }

    private Message(Guid id, Guid channelId, Guid authorId, MessageContent content, DateTime postedAt)
    {
        Id = id;
        ChannelId = channelId;
        AuthorId = authorId;
        Content = content;
        PostedAt = postedAt;
    }

    public static Message Create(Guid channelId, Guid authorId, string? content)
    {
        ValidateReferences(channelId, authorId);
        return new Message(Guid.NewGuid(), channelId, authorId, MessageContent.Create(content), DateTime.UtcNow);
    }

    public static Message Restore(Guid id, Guid channelId, Guid authorId, string content, DateTime postedAt)
    {
        ValidateReferences(channelId, authorId);
        if (id == Guid.Empty || postedAt.Kind != DateTimeKind.Utc)
            throw new DomainException("A stored message needs an ID and a UTC posting timestamp.");
        return new Message(id, channelId, authorId, MessageContent.Create(content), postedAt);
    }

    public void UpdateContent(string? content) => Content = MessageContent.Create(content);

    private static void ValidateReferences(Guid channelId, Guid authorId)
    {
        if (channelId == Guid.Empty) throw new DomainException("ChannelId must not be empty.");
        if (authorId == Guid.Empty) throw new DomainException("AuthorId must not be empty.");
    }
}
