namespace Bizcord.Shared.Contracts.Requests;

public sealed record CreateMessageRequest(Guid ChannelId, Guid AuthorId, string Content);

public sealed record UpdateMessageRequest(string Content);
