namespace FaithTechTorontoAiBuildEvent.Domain.EventFlow;

public sealed class EventState
{
    public const int SingletonId = 1;

    public int Id { get; set; } = SingletonId;
    public long Version { get; set; }
    public EventScreen CurrentScreen { get; set; } = EventScreen.Countdown;
    public bool TeamsFormed { get; set; }
    public long NextParticipantLabel { get; set; } = 1;
    public long NextTeamLabel { get; set; } = 1;
}
