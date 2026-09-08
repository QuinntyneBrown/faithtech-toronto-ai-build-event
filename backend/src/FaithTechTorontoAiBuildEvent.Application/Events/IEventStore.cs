namespace FaithTechTorontoAiBuildEvent.Application.Events;

public interface IEventStore
{
    Task<EventSummary> SaveDraft(SaveEventCommand command, CancellationToken cancellationToken);
    Task<EventSummary?> GetEvent(Guid eventId, CancellationToken cancellationToken);
    Task<EventSummary> CreateDraft(Guid actorId, Guid operationId, string? title, CancellationToken cancellationToken);
    Task<IReadOnlyList<EventSummary>> ListEvents(CancellationToken cancellationToken);
}
