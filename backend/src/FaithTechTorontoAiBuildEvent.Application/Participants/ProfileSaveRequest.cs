namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed record ProfileSaveRequest(Guid OperationId, byte[] InputDigest, byte[] SecretDigest, long ExpectedVersion, ProfileInput Input, DateTimeOffset NowUtc);
