namespace FaithTechTorontoAiBuildEvent.Domain.Access;

public sealed class AdministratorSession
{
    public Guid Id { get; set; }
    public required byte[] SecretDigest { get; set; }
    public long CredentialRevision { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset LastInteractionAtUtc { get; set; }
    public bool Revoked { get; set; }
}
