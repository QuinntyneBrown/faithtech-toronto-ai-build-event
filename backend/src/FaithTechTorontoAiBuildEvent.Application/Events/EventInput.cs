namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed record EventInput(string? Title, string? VenueName, string? Address, double? Latitude, double? Longitude,
    string? WaitingContent, string? ClosingContent, string? DirectionsUrl);
