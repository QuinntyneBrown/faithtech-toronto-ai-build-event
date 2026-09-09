namespace FaithTechTorontoAiBuildEvent.Application.Operations;

public interface IEventImportStore
{
    Task<EventImportReview> Review(Guid eventId, bool create, string? version, EventSeedEnvelope seed, CancellationToken token);
    Task<EventImportResult> Apply(Guid operationId, EventImportReview review, CancellationToken token);
    Task<EventImportResult?> Reconcile(Guid operationId, CancellationToken token);
}
