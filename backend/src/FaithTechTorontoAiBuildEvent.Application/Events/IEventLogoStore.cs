namespace FaithTechTorontoAiBuildEvent.Application.Events;

public interface IEventLogoStore
{
    Task<EventDetail> Save(Guid actorId, Guid eventId, Guid operationId, string version, string payloadHash, LogoContent logo, CancellationToken cancellationToken);
    Task<LogoContent?> Get(Guid eventId, CancellationToken cancellationToken);
}
