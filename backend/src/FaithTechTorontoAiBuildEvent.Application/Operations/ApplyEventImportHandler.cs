using MediatR;
namespace FaithTechTorontoAiBuildEvent.Application.Operations;
public sealed class ApplyEventImportHandler(IEventImportStore store) : IRequestHandler<ApplyEventImportCommand, EventImportResult>
{
    public Task<EventImportResult> Handle(ApplyEventImportCommand request, CancellationToken token) => store.Apply(request.OperationId, request.Review, token);
}
