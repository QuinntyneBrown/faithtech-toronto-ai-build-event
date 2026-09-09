namespace FaithTechTorontoAiBuildEvent.Application.Operations;

public sealed record EventImportResult(Guid EventId, string Version, bool Published, int StageCount);
