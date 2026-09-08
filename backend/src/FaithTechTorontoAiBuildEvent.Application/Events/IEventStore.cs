namespace FaithTechTorontoAiBuildEvent.Application.Events;

public interface IEventStore
{
    Task<EventDetail> SaveDraft(SaveEventCommand command, CancellationToken cancellationToken);
    Task<EventDetail?> GetEvent(Guid eventId, CancellationToken cancellationToken);
    Task<EventSummary> CreateDraft(Guid actorId, Guid operationId, string? title, CancellationToken cancellationToken);
    Task<IReadOnlyList<EventSummary>> ListEvents(CancellationToken cancellationToken);
}
