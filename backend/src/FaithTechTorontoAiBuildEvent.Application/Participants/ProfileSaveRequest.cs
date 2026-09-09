namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed record ProfileSaveRequest(byte[] SecretDigest, long ExpectedVersion, ProfileInput Input, DateTimeOffset NowUtc);
