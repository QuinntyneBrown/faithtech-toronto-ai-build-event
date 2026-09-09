namespace FaithTechTorontoAiBuildEvent.Application.EventState;

public sealed record PublicEventSnapshot(
    string Version,
    string CurrentScreen,
    string Title,
    string Welcome,
    string Purpose,
    string Venue,
    string EventTime,
    DateTimeOffset CountdownTargetUtc,
    IReadOnlyList<ProjectCard> Projects,
    IReadOnlyList<PublicTeam> Teams);
