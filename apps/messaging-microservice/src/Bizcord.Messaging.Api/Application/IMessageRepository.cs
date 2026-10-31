using Bizcord.Messaging.Api.Domain;

namespace Bizcord.Messaging.Api.Application;

public interface IMessageRepository
{
    Task SaveAsync(Message message, CancellationToken cancellationToken);
    Task<Message?> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Message>> ListAsync(Guid? channelId, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Message message, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
