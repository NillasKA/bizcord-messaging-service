namespace Bizcord.Messaging.Api.Domain;

public sealed record MessageContent
{
    public const int MaximumLength = 2000;
    public string Value { get; }

    private MessageContent(string value) => Value = value;

    public static MessageContent Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Message content must not be empty or whitespace.");
        if (value.Length > MaximumLength)
            throw new DomainException($"Message content must not exceed {MaximumLength} characters.");
        return new MessageContent(value);
    }
}
