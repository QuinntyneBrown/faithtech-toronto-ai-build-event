namespace FaithTechTorontoAiBuildEvent.Domain.Events;

public sealed class BuildEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Title { get; set; }
    public string? VenueName { get; set; }
    public string? Address { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? WaitingContent { get; set; }
    public string? ClosingContent { get; set; }
    public string? DirectionsUrl { get; set; }
    public bool Published { get; set; }
    public bool UseLiturgy { get; set; }
}
