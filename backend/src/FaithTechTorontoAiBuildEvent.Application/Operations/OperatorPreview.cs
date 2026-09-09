namespace FaithTechTorontoAiBuildEvent.Application.Operations;

public sealed record OperatorPreview(int SchemaVersion, Guid PreviewId, Guid OperationId, string Kind, OperatorPrincipal Principal,
    DateTimeOffset CreatedAtUtc, string AssemblyHash, string? FilePath, string? FileHash, EventImportReview? Import,
    string[] AppliedMigrations, string[] PendingMigrations);
