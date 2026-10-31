namespace Bizcord.Shared.Contracts.Events;

// This class and these five fields are the agreement specified in the Week 40 lab.
public class MessagePostedEvent
{
    public Guid MessageId { get; set; }
    public Guid ChannelId { get; set; }
    public Guid AuthorId { get; set; }
    public required string Content { get; set; }
    public DateTime PostedAt { get; set; }
}
