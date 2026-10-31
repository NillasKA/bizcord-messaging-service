namespace Bizcord.Messaging.Api.Domain;

public sealed class DomainException(string message) : Exception(message);
