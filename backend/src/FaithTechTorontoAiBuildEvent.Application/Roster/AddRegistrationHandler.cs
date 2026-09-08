using FaithTechTorontoAiBuildEvent.Application.Validation;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Roster;

public sealed class AddRegistrationHandler(IRosterStore store) : IRequestHandler<AddRegistrationCommand, RosterIssuance>
{
    public Task<RosterIssuance> Handle(AddRegistrationCommand request, CancellationToken cancellationToken)
    {
        var name = TextValidation.Text(request.Input.DisplayName, "displayName", 200) ?? throw new InputValidationException("displayName", "Enter a participant name.");
        return store.Add(request with { Input = new(name) }, cancellationToken);
    }
}
