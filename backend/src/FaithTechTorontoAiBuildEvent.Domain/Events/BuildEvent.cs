namespace FaithTechTorontoAiBuildEvent.Domain.Events;

public sealed class BuildEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Title { get; set; }
    public bool Published { get; set; }
    public bool UseLiturgy { get; set; }
}
