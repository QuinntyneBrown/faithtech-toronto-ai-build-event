using FaithTechTorontoAiBuildEvent.Application.Participants;
using FaithTechTorontoAiBuildEvent.Domain.Participants;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Participants;

public sealed class SqlEntryReceiptStore(CompanionDbContext database) : IEntryReceiptStore
{
    public async Task<EntryReceipt?> FindActiveAsync(byte[] secretDigest, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var candidates = await database.EntryReceipts
            .Where(receipt => !receipt.Revoked && receipt.ExpiresAtUtc > nowUtc)
            .ToListAsync(cancellationToken);
        return candidates.SingleOrDefault(receipt => System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(receipt.SecretDigest, secretDigest));
    }

    public async Task AddAsync(EntryReceipt receipt, CancellationToken cancellationToken)
    {
        database.EntryReceipts.Add(receipt);
        await database.SaveChangesAsync(cancellationToken);
    }
}
