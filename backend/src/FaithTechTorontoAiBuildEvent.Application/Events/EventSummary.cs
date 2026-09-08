namespace FaithTechTorontoAiBuildEvent.Application.Events;

public sealed record EventSummary(Guid Id, string? Title, bool Published, bool UseLiturgy, string Version,
    string? Timezone = null, LocalTimeInput? Start = null, LocalTimeInput? End = null,
    DateTimeOffset? StartsAtUtc = null, DateTimeOffset? EndsAtUtc = null);
