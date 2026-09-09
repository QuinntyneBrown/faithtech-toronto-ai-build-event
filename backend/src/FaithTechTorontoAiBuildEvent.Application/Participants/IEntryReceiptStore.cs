using FaithTechTorontoAiBuildEvent.Domain.Participants;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public interface IEntryReceiptStore
{
    Task<EntryReceipt?> FindActiveAsync(byte[] secretDigest, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    Task AddAsync(EntryReceipt receipt, CancellationToken cancellationToken);
}
