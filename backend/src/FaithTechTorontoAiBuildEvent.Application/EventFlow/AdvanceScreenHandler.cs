using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Participants;
using MediatR;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using System.Globalization;

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
            throw new UnauthorizedAccessException();
        }
        if (!await authorizationStore.IsAuthorizedAsync(secretService.Digest(request.AdministratorSecret), DateTimeOffset.UtcNow, cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }
        var inputDigest = OperationInputDigest.Create(
            expectedVersion.ToString(CultureInfo.InvariantCulture),
            request.FromScreen.ToLowerInvariant(),
            request.ToScreen.ToLowerInvariant());
        if (!await eventFlowStore.AdvanceAsync(request.OperationId, inputDigest, expectedVersion, request.FromScreen, request.ToScreen, cancellationToken))
        {
            throw new InvalidOperationException("Event state changed; reload and try again.");
        }
        return true;
    }
}
