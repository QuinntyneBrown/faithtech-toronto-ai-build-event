namespace FaithTechTorontoAiBuildEvent.Domain.Roster;

public sealed class Registration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public string DisplayName { get; set; } = "";
    public bool Active { get; set; } = true;
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public string CodeDigest { get; set; } = "";
    public Guid CredentialVersion { get; set; } = Guid.NewGuid();
    public DateTimeOffset CodeIssuedAtUtc { get; set; }
    public DateTimeOffset? FirstAccessAtUtc { get; set; }
}
