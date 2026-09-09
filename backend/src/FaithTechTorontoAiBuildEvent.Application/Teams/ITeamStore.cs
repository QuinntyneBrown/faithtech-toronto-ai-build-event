namespace FaithTechTorontoAiBuildEvent.Application.Teams;

public interface ITeamStore
{
    Task MoveAsync(Guid operationId, byte[] inputDigest, Guid participantId, string destination, Guid? teamId, long expectedVersion, CancellationToken cancellationToken);
    Task AssignProjectAsync(Guid operationId, byte[] inputDigest, Guid teamId, Guid? projectId, long expectedVersion, CancellationToken cancellationToken);
}
