namespace FaithTechTorontoAiBuildEvent.Domain.Operations;

public sealed class EventOperationReceipt
{
    public Guid OperationId { get; set; }
    public required string OperationKind { get; set; }
    public required byte[] InputDigest { get; set; }
    public Guid? ResultId { get; set; }
    public long ResultVersion { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
