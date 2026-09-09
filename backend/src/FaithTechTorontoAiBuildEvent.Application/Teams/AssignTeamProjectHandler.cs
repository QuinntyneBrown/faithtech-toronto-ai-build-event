using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Participants;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Teams;

public sealed class AssignTeamProjectHandler(IAdministratorAuthorizationStore authorizationStore, IEntryReceiptSecretService secretService, ITeamStore teamStore) : IRequestHandler<AssignTeamProjectCommand>
{
    public async Task Handle(AssignTeamProjectCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AdministratorSecret) || !long.TryParse(request.ExpectedVersion, out var version) || !await authorizationStore.IsAuthorizedAsync(secretService.Digest(request.AdministratorSecret), DateTimeOffset.UtcNow, cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }
        await teamStore.AssignProjectAsync(request.TeamId, request.ProjectId, version, cancellationToken);
    }
}
