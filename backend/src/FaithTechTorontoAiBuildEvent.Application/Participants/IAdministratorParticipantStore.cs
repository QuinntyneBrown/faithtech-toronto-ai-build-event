namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public interface IAdministratorParticipantStore
{
    Task<IReadOnlyList<AdministratorParticipant>> ListAsync(CancellationToken cancellationToken);
    Task<AdministratorParticipant> AddAsync(Guid operationId, byte[] inputDigest, string email, string normalizedEmail, long expectedVersion, CancellationToken cancellationToken);
    Task DeleteAsync(Guid operationId, byte[] inputDigest, Guid participantId, long expectedVersion, CancellationToken cancellationToken);
    Task<AdministratorParticipant> UpdateAsync(Guid operationId, byte[] inputDigest, Guid participantId, string email, string normalizedEmail, AdministratorParticipantInput input, long expectedVersion, CancellationToken cancellationToken);
}
