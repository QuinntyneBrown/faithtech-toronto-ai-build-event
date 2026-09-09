using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Participants;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Teams;

public sealed class MoveTeamMemberHandler(
    IAdministratorAuthorizationStore authorizationStore,
    IEntryReceiptSecretService secretService,
    ITeamStore teamStore) : IRequestHandler<MoveTeamMemberCommand>
{
    public async Task Handle(MoveTeamMemberCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AdministratorSecret) || !long.TryParse(request.ExpectedVersion, out var expectedVersion)
            || !await authorizationStore.IsAuthorizedAsync(secretService.Digest(request.AdministratorSecret), DateTimeOffset.UtcNow, cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }

        await teamStore.MoveAsync(request.ParticipantId, request.Destination, request.TeamId, expectedVersion, cancellationToken);
    }
}
