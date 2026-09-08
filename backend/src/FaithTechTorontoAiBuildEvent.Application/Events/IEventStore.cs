namespace FaithTechTorontoAiBuildEvent.Application.Events;

public interface IEventStore
{
    Task<EventDetail> SaveDraft(SaveEventCommand command, CancellationToken cancellationToken);
    Task<EventDetail?> GetEvent(Guid eventId, CancellationToken cancellationToken);
    Task<EventSummary> CreateDraft(Guid actorId, Guid operationId, string? title, CancellationToken cancellationToken);
    Task<EventDetail> CopyEvent(Guid actorId, Guid sourceEventId, Guid operationId, LocalTimeInput newStart, CancellationToken cancellationToken);
    Task<IReadOnlyList<EventSummary>> ListEvents(CancellationToken cancellationToken);
    Task<EntryHeader?> GetPublishedHeader(Guid eventId, CancellationToken cancellationToken);
    Task<string> GetAuthorizedInitialRoute(Guid eventId, string? requestedReturnTo, CancellationToken cancellationToken);
}
