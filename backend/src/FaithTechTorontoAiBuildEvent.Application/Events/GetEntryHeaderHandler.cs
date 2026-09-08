using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed class GetEntryHeaderHandler(IEventStore store) : IRequestHandler<GetEntryHeaderQuery, EntryHeader?>
{
    public Task<EntryHeader?> Handle(GetEntryHeaderQuery request, CancellationToken cancellationToken) =>
        store.GetPublishedHeader(request.EventId, cancellationToken);
}
