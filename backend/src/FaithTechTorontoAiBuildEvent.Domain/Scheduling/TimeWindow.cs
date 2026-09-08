namespace FaithTechTorontoAiBuildEvent.Domain.Scheduling;

public sealed class TimeWindow
{
    public DateTime StartLocal { get; set; }
    public DateTime EndLocal { get; set; }
    public int StartOffsetMinutes { get; set; }
    public int EndOffsetMinutes { get; set; }
    public DateTimeOffset StartsAtUtc { get; set; }
    public DateTimeOffset EndsAtUtc { get; set; }
}
