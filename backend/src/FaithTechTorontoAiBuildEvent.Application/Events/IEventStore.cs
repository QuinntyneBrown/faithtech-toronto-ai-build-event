namespace FaithTechTorontoAiBuildEvent.Application.Events;

public interface IEventStore
{
    Task<EventSummary> CreateDraft(Guid actorId, Guid operationId, string? title, CancellationToken cancellationToken);
    Task<IReadOnlyList<EventSummary>> ListEvents(CancellationToken cancellationToken);
}
