using MediatR;
using FaithTechTorontoAiBuildEvent.Application.Access;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed class EnterParticipantHandler(IEntryStore entryStore, IEntryReceiptSecretService secretService, IPublicEntryAttemptStore attempts, ISourceDigestService sourceDigestService)
    : IRequestHandler<EnterParticipantCommand, EntryResult>
{
    public async Task<EntryResult> Handle(EnterParticipantCommand request, CancellationToken cancellationToken)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var source = sourceDigestService.Digest(request.Source);
        var retryAfter = await attempts.GetRetryAfterAsync(source, nowUtc, cancellationToken);
        if (retryAfter is not null) throw new PublicEntryThrottledException(retryAfter.Value);
        await attempts.RecordAsync(source, nowUtc, cancellationToken);
        if (string.IsNullOrWhiteSpace(request.ReceiptSecret) || !long.TryParse(request.ExpectedVersion, out var expectedVersion))
        {
            throw new EntryValidationException("Entry receipt or event version is invalid.");
        }

        var email = request.Input.Email.Trim();
        var normalizedEmail = EmailNormalizer.Normalize(email);
        var sessionSecret = secretService.CreateSecret();
        var result = await entryStore.EnterAsync(
            new EntryStoreRequest(
                request.OperationId,
                expectedVersion,
                email,
                normalizedEmail,
                secretService.Digest(request.ReceiptSecret),
                secretService.Digest(sessionSecret),
                nowUtc),
            cancellationToken);
        return new EntryResult(result.ParticipantId, result.PublicLabel, sessionSecret);
    }
}
