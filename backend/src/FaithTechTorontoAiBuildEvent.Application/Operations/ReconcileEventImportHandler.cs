using MediatR;
namespace FaithTechTorontoAiBuildEvent.Application.Operations;
public sealed class ReconcileEventImportHandler(IEventImportStore store) : IRequestHandler<ReconcileEventImportQuery, EventImportResult?>
{
    public Task<EventImportResult?> Handle(ReconcileEventImportQuery request, CancellationToken token) => store.Reconcile(request.OperationId, token);
}
