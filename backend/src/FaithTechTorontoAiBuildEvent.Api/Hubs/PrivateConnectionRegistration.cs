namespace FaithTechTorontoAiBuildEvent.Api.Hubs;

public sealed record PrivateConnectionRegistration(
    string ConnectionId,
    PrivateSessionKind SessionKind,
    byte[] SecretDigest);
