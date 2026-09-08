namespace FaithTechTorontoAiBuildEvent.Domain.Scheduling;

public sealed class EventStage
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = "";
    public string? Phase { get; set; }
    public string ScreenType { get; set; } = "information";
    public string? Content { get; set; }
    public string? ResourceUrl { get; set; }
    public TimeWindow Interval { get; set; } = new();
}
