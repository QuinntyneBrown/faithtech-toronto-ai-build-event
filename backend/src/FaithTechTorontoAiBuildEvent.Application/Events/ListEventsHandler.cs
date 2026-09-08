using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed class ListEventsHandler(IEventStore store) : IRequestHandler<ListEventsQuery, IReadOnlyList<EventSummary>>
{
    public Task<IReadOnlyList<EventSummary>> Handle(ListEventsQuery request, CancellationToken cancellationToken) =>
        store.ListEvents(cancellationToken);
}
