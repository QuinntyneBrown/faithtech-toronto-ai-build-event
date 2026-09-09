using FaithTechTorontoAiBuildEvent.Application.EventState;

namespace FaithTechTorontoAiBuildEvent.Application.EventState;

public interface IEventStateStore
{
    Task<PublicEventSnapshot> GetPublicSnapshotAsync(CancellationToken cancellationToken);
    Task<DateTimeOffset> GetServerTimeAsync(CancellationToken cancellationToken);
    Task<bool> IsReadyAsync(CancellationToken cancellationToken);
}
