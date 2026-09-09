namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public interface IAdministratorParticipantStore
{
    Task<IReadOnlyList<AdministratorParticipant>> ListAsync(CancellationToken cancellationToken);
    Task<AdministratorParticipant> AddAsync(string email, string normalizedEmail, long expectedVersion, CancellationToken cancellationToken);
}
