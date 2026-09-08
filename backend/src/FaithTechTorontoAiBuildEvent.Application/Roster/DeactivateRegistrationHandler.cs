using FaithTechTorontoAiBuildEvent.Application.Operations;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public sealed class DeactivateRegistrationHandler(IRosterStore store) : IRequestHandler<DeactivateRegistrationCommand, RosterEntry>
{
    public Task<RosterEntry> Handle(DeactivateRegistrationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Version)) throw new VersionRequiredException();
        return store.Deactivate(request with { Version = request.Version.Trim('"') }, cancellationToken);
    }
}
