using FaithTechTorontoAiBuildEvent.Domain.Participants;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed class CreateEntryReceiptHandler(
    IEntryReceiptStore receiptStore,
    IEntryReceiptSecretService secretService)
    : IRequestHandler<CreateEntryReceiptCommand, EntryReceiptBootstrap>
{
    public async Task<EntryReceiptBootstrap> Handle(CreateEntryReceiptCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        if (!string.IsNullOrEmpty(request.ExistingSecret))
        {
            var existing = await receiptStore.FindActiveAsync(secretService.Digest(request.ExistingSecret), now, cancellationToken);
            if (existing is not null)
            {
                return new EntryReceiptBootstrap(existing.OperationId, request.ExistingSecret);
            }
        }

        var secret = secretService.CreateSecret();
        var receipt = new EntryReceipt
        {
            Id = Guid.NewGuid(),
            SecretDigest = secretService.Digest(secret),
            OperationId = Guid.NewGuid(),
            ExpiresAtUtc = now.AddHours(8)
        };
        await receiptStore.AddAsync(receipt, cancellationToken);
        return new EntryReceiptBootstrap(receipt.OperationId, secret);
    }
}
