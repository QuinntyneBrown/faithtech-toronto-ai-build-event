using FaithTechTorontoAiBuildEvent.Application.Operations;
namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed record OperatorResult(Guid? OperationId, Guid? PreviewId, OperatorPrincipal? Principal, string Outcome,
    double DurationMs, object? Result, string? Error)
{
    public object? Target => Principal is null ? null : new { Principal.Server, Principal.Database, Principal.Environment };
}
