namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed record EventDetail(Guid Id, string? Title, bool Published, bool UseLiturgy, string Version,
    string? VenueName, string? Address, double? Latitude, double? Longitude,
    string? WaitingContent, string? ClosingContent, string? DirectionsUrl);
