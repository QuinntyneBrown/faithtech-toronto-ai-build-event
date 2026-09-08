using FaithTechTorontoAiBuildEvent.Application.Operations;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public sealed class ReactivateRegistrationHandler(IRosterStore store) : IRequestHandler<ReactivateRegistrationCommand, RosterEntry>
{
    public Task<RosterEntry> Handle(ReactivateRegistrationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Version)) throw new VersionRequiredException();
        return store.Reactivate(request with { Version = request.Version.Trim('"') }, cancellationToken);
    }
}
