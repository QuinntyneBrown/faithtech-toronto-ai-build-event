using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class OperatorTargetProfileStore(string path)
{
    private static readonly JsonSerializerOptions options = new() { WriteIndented = true };

    public OperatorTargetProfile Load(string name)
    {
        var document = Read();
        if (!document.Targets.TryGetValue(name, out var profile))
            throw new InvalidOperationException($"Target '{name}' is not configured.");
        return profile;
    }

    public void Save(string name, OperatorTargetProfile profile)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(profile.Server) ||
            string.IsNullOrWhiteSpace(profile.Database) || string.IsNullOrWhiteSpace(profile.Environment))
            throw new ArgumentException("Target name, server, database, and environment are required.");
        var document = Read();
        document.Targets[name] = profile;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(document, options));
    }

    private OperatorTargetProfileDocument Read() => File.Exists(path)
        ? JsonSerializer.Deserialize<OperatorTargetProfileDocument>(File.ReadAllText(path)) ?? throw new InvalidOperationException("Target configuration is invalid.")
        : new OperatorTargetProfileDocument();
}
