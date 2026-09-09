namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public interface IProfileStore
{
    Task<ParticipantProfile?> GetAsync(byte[] secretDigest, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    Task<ParticipantProfile?> SaveAsync(ProfileSaveRequest request, CancellationToken cancellationToken);
}
