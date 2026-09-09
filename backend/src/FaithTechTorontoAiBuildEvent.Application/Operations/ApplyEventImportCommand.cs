using MediatR;
namespace FaithTechTorontoAiBuildEvent.Application.Operations;
public sealed record ApplyEventImportCommand(Guid OperationId, EventImportReview Review) : IRequest<EventImportResult>;
