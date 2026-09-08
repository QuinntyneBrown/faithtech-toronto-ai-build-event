namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record EntryResult(Guid ParticipantId, Guid EventId, DateTimeOffset AbsoluteExpiresAtUtc, string AuthorizedInitialRoute);
