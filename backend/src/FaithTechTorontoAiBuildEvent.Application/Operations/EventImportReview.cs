namespace FaithTechTorontoAiBuildEvent.Application.Operations;

public sealed record EventImportReview(Guid EventId, bool Create, string? Version, EventSeedEnvelope Seed,
    EventSeedState? Before, EventSeedState After, bool Published);
