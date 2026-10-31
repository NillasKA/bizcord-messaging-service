namespace Bizcord.Shared.Contracts.Models;

public sealed record MessageDto(
    Guid MessageId,
    Guid ChannelId,
    Guid AuthorId,
    string Content,
    DateTime PostedAt);
