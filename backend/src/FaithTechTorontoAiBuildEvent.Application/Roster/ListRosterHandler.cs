using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public sealed class ListRosterHandler(IRosterStore store) : IRequestHandler<ListRosterQuery, IReadOnlyList<RosterEntry>>
{
    public Task<IReadOnlyList<RosterEntry>> Handle(ListRosterQuery request, CancellationToken cancellationToken) => store.List(request.EventId, cancellationToken);
}
