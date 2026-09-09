using FaithTechTorontoAiBuildEvent.Application.Operations;
namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed record OperatorResult(Guid? OperationId, Guid? PreviewId, OperatorPrincipal? Principal, string Outcome,
    double DurationMs, object? Result, [property: System.Text.Json.Serialization.JsonIgnore] string? Message)
{
    public int SchemaVersion => 1;
    public object? Error => Message is null ? null : new { Code = Outcome, Message };
    public object? Target => Principal is null ? null : new { Principal.Server, Principal.Database, Principal.Environment };
}
