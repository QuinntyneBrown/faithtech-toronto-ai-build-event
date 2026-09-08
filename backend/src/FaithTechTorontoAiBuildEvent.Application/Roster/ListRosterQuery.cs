using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public sealed record ListRosterQuery(Guid EventId) : IRequest<IReadOnlyList<RosterEntry>>;
