namespace FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;

public sealed class EventOptions
{
    public const string SectionName = "Event";

    public string Title { get; init; } = "FaithTech Toronto AI Build Event";
    public string Welcome { get; init; } = "Welcome to the Toronto AI Build Event.";
    public string Purpose { get; init; } = "Enter your email for the raffle, then build and demo together.";
    public string Venue { get; init; } = "Stone Church — Davenport Community Campus, 45 Davenport Rd, Toronto";
    public string EventTime { get; init; } = "September 9, 2026, 5:00–9:00 PM America/Toronto";
    public DateTimeOffset CountdownTargetUtc { get; init; } = new(2026, 9, 9, 21, 20, 0, TimeSpan.Zero);
}
