using FaithTechTorontoAiBuildEvent.Application.Participants;
using FaithTechTorontoAiBuildEvent.Domain.EventFlow;
using FaithTechTorontoAiBuildEvent.Domain.Participants;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Participants;

public sealed class SqlEntryStore(CompanionDbContext database) : IEntryStore
{
    public async Task<EntryStoreResult> EnterAsync(EntryStoreRequest request, CancellationToken cancellationToken)
    {
        var receipt = await FindReceiptAsync(request.ReceiptDigest, request.NowUtc, cancellationToken)
            ?? throw new EntryValidationException("Entry receipt is no longer valid.");
        if (receipt.OperationId != request.OperationId)
        {
            throw new EntryValidationException("Entry operation does not match its receipt.");
        }
        if (receipt.ParticipantId is not null)
        {
            var replayedParticipant = await database.Participants.SingleAsync(participant => participant.Id == receipt.ParticipantId, cancellationToken);
            database.ParticipantSessions.Add(new ParticipantSession
            {
                Id = Guid.NewGuid(),
                ParticipantId = replayedParticipant.Id,
                SecretDigest = request.SessionDigest,
                ExpiresAtUtc = receipt.ExpiresAtUtc
            });
            await database.SaveChangesAsync(cancellationToken);
            return new EntryStoreResult(replayedParticipant.Id, replayedParticipant.PublicLabel);
        }

        var state = await database.EventStates.SingleAsync(cancellationToken);
        if (state.CurrentScreen != EventScreen.Countdown)
        {
            throw new EntryValidationException("Entry is closed; ask an administrator for help.");
        }
        if (state.Version != request.ExpectedVersion)
        {
            throw new EntryValidationException("Event state changed; reload and try again.");
        }
        if (await database.Participants.AnyAsync(participant => participant.NormalizedEmail == request.NormalizedEmail, cancellationToken))
        {
            throw new EntryValidationException("This email is already entered; ask an administrator to update your details.");
        }

        var participant = new Participant
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            NormalizedEmail = request.NormalizedEmail,
            PublicLabel = $"Participant {state.NextParticipantLabel:D3}"
        };
        state.NextParticipantLabel++;
        state.Version++;
        receipt.ParticipantId = participant.Id;
        database.Participants.Add(participant);
        database.ParticipantSessions.Add(new ParticipantSession
        {
            Id = Guid.NewGuid(),
            ParticipantId = participant.Id,
            SecretDigest = request.SessionDigest,
            ExpiresAtUtc = receipt.ExpiresAtUtc
        });
        await database.SaveChangesAsync(cancellationToken);
        return new EntryStoreResult(participant.Id, participant.PublicLabel);
    }

    private async Task<EntryReceipt?> FindReceiptAsync(byte[] digest, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var receipts = await database.EntryReceipts.Where(receipt => !receipt.Revoked && receipt.ExpiresAtUtc > nowUtc).ToListAsync(cancellationToken);
        return receipts.SingleOrDefault(receipt => System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(receipt.SecretDigest, digest));
    }
}
