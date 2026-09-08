namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class OperatorTargetProfileDocument
{
    public int SchemaVersion { get; set; } = 1;
    public Dictionary<string, OperatorTargetProfile> Targets { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
