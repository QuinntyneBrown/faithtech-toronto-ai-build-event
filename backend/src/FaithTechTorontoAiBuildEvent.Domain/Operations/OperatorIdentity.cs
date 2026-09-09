namespace FaithTechTorontoAiBuildEvent.Domain.Operations;

public sealed class OperatorIdentity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string PrincipalKey { get; set; }
}
