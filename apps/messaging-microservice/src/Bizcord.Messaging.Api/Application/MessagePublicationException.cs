namespace Bizcord.Messaging.Api.Application;

public sealed class MessagePublicationException(Guid messageId, Exception innerException)
    : Exception("The message was saved, but event publication could not be confirmed.", innerException)
{
    public Guid MessageId { get; } = messageId;
}
