namespace FaithTechTorontoAiBuildEvent.Domain.Access;

public sealed class CompanionCredential
{
    public const int SingletonId = 1;

    public int Id { get; set; } = SingletonId;
    public required byte[] Salt { get; set; }
    public required byte[] Verifier { get; set; }
    public long Revision { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
}
