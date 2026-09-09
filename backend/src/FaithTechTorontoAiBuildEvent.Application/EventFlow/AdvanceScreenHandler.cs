using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Participants;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.EventFlow;

public sealed class AdvanceScreenHandler(
    IAdministratorAuthorizationStore authorizationStore,
    IEntryReceiptSecretService secretService,
    IEventFlowStore eventFlowStore)
    : IRequestHandler<AdvanceScreenCommand, bool>
{
    public async Task<bool> Handle(AdvanceScreenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AdministratorSecret) || !long.TryParse(request.ExpectedVersion, out var expectedVersion))
        {
            return false;
        }
        if (!await authorizationStore.IsAuthorizedAsync(secretService.Digest(request.AdministratorSecret), DateTimeOffset.UtcNow, cancellationToken))
        {
            return false;
        }
        return await eventFlowStore.AdvanceAsync(expectedVersion, request.FromScreen, request.ToScreen, cancellationToken);
    }
}
