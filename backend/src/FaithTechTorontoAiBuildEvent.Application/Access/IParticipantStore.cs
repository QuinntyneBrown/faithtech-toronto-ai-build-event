using FaithTechTorontoAiBuildEvent.Domain.Access;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public interface IParticipantStore
{
    Task<ParticipantSession?> Authenticate(Guid eventId, string email, string normalizedEmail, string entryCode, string source, CancellationToken cancellationToken);
    Task<ParticipantSession?> FindSession(Guid sessionId, CancellationToken cancellationToken);
    Task<DateTimeOffset> GetUtcNow(CancellationToken cancellationToken);
    Task<bool> IsActiveRegistration(Guid registrationId, CancellationToken cancellationToken);
    Task RevokeSession(Guid sessionId, CancellationToken cancellationToken);
}
