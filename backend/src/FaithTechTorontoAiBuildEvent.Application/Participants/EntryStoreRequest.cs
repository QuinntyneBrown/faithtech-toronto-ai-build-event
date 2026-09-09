namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed record EntryStoreRequest(
    Guid OperationId,
    long ExpectedVersion,
    string Email,
    string NormalizedEmail,
    byte[] ReceiptDigest,
    byte[] SessionDigest,
    DateTimeOffset NowUtc);
