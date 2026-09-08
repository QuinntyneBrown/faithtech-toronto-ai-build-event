using FaithTechTorontoAiBuildEvent.Application.Operations;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public sealed class ReplaceEntryCodeHandler(IRosterStore store) : IRequestHandler<ReplaceEntryCodeCommand, RosterIssuance>
{
    public Task<RosterIssuance> Handle(ReplaceEntryCodeCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Version)) throw new VersionRequiredException();
        return store.ReplaceCode(request with { Version = request.Version.Trim('"') }, cancellationToken);
    }
}
