namespace FaithTechTorontoAiBuildEvent.Application.Teams;

public interface ITeamStore
{
    Task MoveAsync(Guid participantId, string destination, Guid? teamId, long expectedVersion, CancellationToken cancellationToken);
}
