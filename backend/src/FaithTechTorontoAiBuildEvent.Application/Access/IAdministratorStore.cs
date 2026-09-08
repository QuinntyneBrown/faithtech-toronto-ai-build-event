using FaithTechTorontoAiBuildEvent.Domain.Access;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public interface IAdministratorStore
{
    Task<Guid?> VerifyCredentials(string username, string password, string source, CancellationToken cancellationToken);
    Task<bool> IsEnabledAdministrator(Guid id, CancellationToken cancellationToken);
    Task<AdministratorSession?> FindSession(Guid id, CancellationToken cancellationToken);
    Task SaveSession(AdministratorSession session, CancellationToken cancellationToken);
    Task<DateTimeOffset> GetUtcNow(CancellationToken cancellationToken);
    Task RevokeSession(Guid sessionId, CancellationToken cancellationToken);
    Task<bool> RecordInteraction(Guid sessionId, CancellationToken cancellationToken);
}
