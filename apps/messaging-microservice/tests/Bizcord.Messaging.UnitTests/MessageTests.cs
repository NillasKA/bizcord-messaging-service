using Bizcord.Messaging.Api.Domain;

namespace Bizcord.Messaging.UnitTests;

public sealed class MessageTests
{
    [Fact]
    public void Create_SetsIdentityReferencesContentAndUtcTime()
    {
        var channelId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var message = Message.Create(channelId, authorId, "Hello world");

        Assert.NotEqual(Guid.Empty, message.Id);
        Assert.Equal(channelId, message.ChannelId);
        Assert.Equal(authorId, message.AuthorId);
        Assert.Equal("Hello world", message.Content.Value);
        Assert.Equal(DateTimeKind.Utc, message.PostedAt.Kind);
        Assert.InRange(message.PostedAt, before, DateTime.UtcNow);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\r\n\t")]
    public void Create_RejectsMissingContent(string? content)
        => Assert.Throws<DomainException>(() => Message.Create(Guid.NewGuid(), Guid.NewGuid(), content));

    [Fact]
    public void Content_EnforcesMaximumLength()
    {
        Assert.Equal(2000, MessageContent.Create(new string('a', 2000)).Value.Length);
        Assert.Throws<DomainException>(() => MessageContent.Create(new string('a', 2001)));
    }

    [Fact]
    public void Create_RejectsEmptyChannelAndAuthor()
    {
        Assert.Throws<DomainException>(() => Message.Create(Guid.Empty, Guid.NewGuid(), "Hello"));
        Assert.Throws<DomainException>(() => Message.Create(Guid.NewGuid(), Guid.Empty, "Hello"));
    }

    [Fact]
    public void Update_ChangesContentWithoutChangingIdentityOrPostingTime()
    {
        var message = Message.Create(Guid.NewGuid(), Guid.NewGuid(), "Original");
        var identity = (message.Id, message.ChannelId, message.AuthorId, message.PostedAt);

        message.UpdateContent("Edited");

        Assert.Equal("Edited", message.Content.Value);
        Assert.Equal(identity, (message.Id, message.ChannelId, message.AuthorId, message.PostedAt));
        Assert.Throws<DomainException>(() => message.UpdateContent(" "));
        Assert.Equal("Edited", message.Content.Value);
    }

    [Fact]
    public void Content_HasValueEquality()
        => Assert.Equal(MessageContent.Create("Hello"), MessageContent.Create("Hello"));
}
