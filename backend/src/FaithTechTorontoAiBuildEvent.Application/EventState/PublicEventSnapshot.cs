namespace FaithTechTorontoAiBuildEvent.Application.EventState;

using FaithTechTorontoAiBuildEvent.Application.Raffle;

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
    IReadOnlyList<PublicTeam> Teams,
    RaffleSnapshot Raffle);
