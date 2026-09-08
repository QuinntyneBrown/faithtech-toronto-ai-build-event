namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed record AdministratorSessionState(Guid ActorId, DateTimeOffset ServerNow,
    DateTimeOffset AbsoluteExpiresAtUtc, DateTimeOffset IdleExpiresAtUtc);
