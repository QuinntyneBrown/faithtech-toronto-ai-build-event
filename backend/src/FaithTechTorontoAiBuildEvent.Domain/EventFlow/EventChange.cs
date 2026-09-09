namespace FaithTechTorontoAiBuildEvent.Domain.EventFlow;

public sealed class EventChange
{
    public long Version { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
}
