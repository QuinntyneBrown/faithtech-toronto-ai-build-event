using MediatR;
namespace FaithTechTorontoAiBuildEvent.Application.Operations;
public sealed record ReconcileEventImportQuery(Guid OperationId) : IRequest<EventImportResult?>;
