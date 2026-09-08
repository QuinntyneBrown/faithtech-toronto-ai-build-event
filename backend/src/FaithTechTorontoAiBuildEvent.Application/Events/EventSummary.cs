namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed record EventSummary(Guid Id, string? Title, bool Published, bool UseLiturgy, string Version);
