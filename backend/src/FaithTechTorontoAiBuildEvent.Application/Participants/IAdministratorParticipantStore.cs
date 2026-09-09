namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public interface IAdministratorParticipantStore
{
    Task<IReadOnlyList<AdministratorParticipant>> ListAsync(CancellationToken cancellationToken);
}
