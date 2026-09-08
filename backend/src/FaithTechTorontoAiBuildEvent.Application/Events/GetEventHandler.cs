using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed class GetEventHandler(IEventStore store) : IRequestHandler<GetEventQuery, EventSummary?>
{
    public Task<EventSummary?> Handle(GetEventQuery request, CancellationToken cancellationToken) =>
        store.GetEvent(request.EventId, cancellationToken);
}
