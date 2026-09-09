namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public interface IParticipantSessionStore
{
    Task<ParticipantSessionState?> FindActiveAsync(byte[] secretDigest, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    Task RevokeAsync(byte[] secretDigest, CancellationToken cancellationToken);
}
