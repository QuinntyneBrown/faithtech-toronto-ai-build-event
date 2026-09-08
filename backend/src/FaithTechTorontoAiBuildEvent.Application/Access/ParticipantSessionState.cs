namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record ParticipantSessionState(Guid ParticipantId, Guid EventId, DateTimeOffset ServerNow, DateTimeOffset AbsoluteExpiresAtUtc);
