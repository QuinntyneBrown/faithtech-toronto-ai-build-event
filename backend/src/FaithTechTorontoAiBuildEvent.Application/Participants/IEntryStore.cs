namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public interface IEntryStore
{
    Task<EntryStoreResult> EnterAsync(EntryStoreRequest request, CancellationToken cancellationToken);
}
