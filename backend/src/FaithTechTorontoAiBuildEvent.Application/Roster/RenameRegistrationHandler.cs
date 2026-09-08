using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Application.Validation;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public sealed class RenameRegistrationHandler(IRosterStore store) : IRequestHandler<RenameRegistrationCommand, RosterEntry>
{
    public Task<RosterEntry> Handle(RenameRegistrationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Version)) throw new VersionRequiredException();
        var name = TextValidation.Text(request.DisplayName, "displayName", 200) ?? throw new InputValidationException("displayName", "Enter a participant name.");
        return store.Rename(request with { DisplayName = name, Version = request.Version.Trim('"') }, cancellationToken);
    }
}
