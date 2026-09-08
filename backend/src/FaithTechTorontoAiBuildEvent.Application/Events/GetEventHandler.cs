using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed class GetEventHandler(IEventStore store) : IRequestHandler<GetEventQuery, EventDetail?>
{
    public Task<EventDetail?> Handle(GetEventQuery request, CancellationToken cancellationToken) =>
        store.GetEvent(request.EventId, cancellationToken);
}
