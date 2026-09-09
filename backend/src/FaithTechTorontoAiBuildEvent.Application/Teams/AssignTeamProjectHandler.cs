using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Participants;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using MediatR;
using System.Globalization;

namespace FaithTechTorontoAiBuildEvent.Application.Teams;

public sealed class AssignTeamProjectHandler(IAdministratorAuthorizationStore authorizationStore, IEntryReceiptSecretService secretService, ITeamStore teamStore) : IRequestHandler<AssignTeamProjectCommand>
{
    public async Task Handle(AssignTeamProjectCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AdministratorSecret) || !long.TryParse(request.ExpectedVersion, out var version) || !await authorizationStore.IsAuthorizedAsync(secretService.Digest(request.AdministratorSecret), DateTimeOffset.UtcNow, cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }
        var inputDigest = OperationInputDigest.Create(
            request.TeamId.ToString("D"),
            request.ProjectId?.ToString("D"),
            version.ToString(CultureInfo.InvariantCulture));
        await teamStore.AssignProjectAsync(request.OperationId, inputDigest, request.TeamId, request.ProjectId, version, cancellationToken);
    }
}
