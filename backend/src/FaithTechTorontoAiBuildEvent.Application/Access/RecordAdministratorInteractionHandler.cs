using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class RecordAdministratorInteractionHandler(IAdministratorStore store)
    : IRequestHandler<RecordAdministratorInteractionCommand, bool>
{
    public Task<bool> Handle(RecordAdministratorInteractionCommand request, CancellationToken cancellationToken) =>
        store.RecordInteraction(request.SessionId, cancellationToken);
}
