using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.Provisioning;

public sealed class OperatorTargetProfileStore(string path)
{
    private static readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web) {
        WriteIndented = true, UnmappedMemberHandling = System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow };

    public OperatorTargetProfile Load(string name)
    {
        var document = Read();
        if (!document.Targets.TryGetValue(name, out var profile))
            throw new InvalidOperationException($"Target '{name}' is not configured.");
        Validate(name, profile);
        return profile;
    }

    public void Save(string name, OperatorTargetProfile profile)
    {
        Validate(name, profile);
        var document = Read();
        document.Targets[name] = profile;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(document, options));
    }

    private OperatorTargetProfileDocument Read()
    {
        if (!Path.IsPathFullyQualified(path)) throw new ArgumentException("Use an absolute config path.");
        if (!File.Exists(path) || new FileInfo(path).Length == 0) return new();
        using var json = JsonDocument.Parse(File.ReadAllText(path));
        CheckKeys(json.RootElement);
        var document = json.RootElement.Deserialize<OperatorTargetProfileDocument>(options) ?? throw new ArgumentException("Invalid profile document.");
        if (document.SchemaVersion != 1 || document.Targets is null) throw new ArgumentException("Unsupported profile document.");
        document.Targets = new(document.Targets, StringComparer.OrdinalIgnoreCase);
        return document;
    }
    private static void Validate(string name, OperatorTargetProfile? profile)
    {
        if (string.IsNullOrWhiteSpace(name) || profile is null || string.IsNullOrWhiteSpace(profile.Server) ||
            string.IsNullOrWhiteSpace(profile.Database) || profile.Environment?.ToLowerInvariant() is not ("production" or "test" or "development"))
            throw new ArgumentException("Target name, server, database, and an explicit environment classification are required.");
    }
    private static void CheckKeys(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object) return;
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var property in element.EnumerateObject()) {
            if (!keys.Add(property.Name)) throw new ArgumentException("Duplicate target configuration key.");
            CheckKeys(property.Value);
        }
    }
}
