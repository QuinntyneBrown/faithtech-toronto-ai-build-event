namespace FaithTechTorontoAiBuildEvent.Api.Access;

public sealed record AuthenticateParticipantRequest(string? Email, string? EntryCode, string? ReturnTo);
