using FaithTechTorontoAiBuildEvent.Application.Validation;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed class CopyEventHandler(IEventStore store) : IRequestHandler<CopyEventCommand, EventDetail>
{
    public Task<EventDetail> Handle(CopyEventCommand request, CancellationToken cancellationToken)
    {
        if (request.NewStart is null) throw new InputValidationException("start", "Enter the new event's start date and time.");
        return store.CopyEvent(request.ActorId, request.SourceEventId, request.OperationId, request.NewStart, cancellationToken);
    }
}
