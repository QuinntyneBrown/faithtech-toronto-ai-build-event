using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Scheduling;
using FaithTechTorontoAiBuildEvent.Application.Validation;

namespace FaithTechTorontoAiBuildEvent.Application.Operations;

public static class EventSeedReader
{
    public static JsonSerializerOptions Json { get; } = new(JsonSerializerDefaults.Web) {
        PropertyNameCaseInsensitive = false, UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        NumberHandling = JsonNumberHandling.Strict };

    public static EventSeedEnvelope Read(byte[] bytes)
    {
        try {
            var text = new UTF8Encoding(false, true).GetString(bytes);
            if (text.StartsWith('\uFEFF')) text = text[1..];
            using var document = JsonDocument.Parse(text);
            CheckDuplicates(document.RootElement, "$");
            JsonElement? settings = null; ScheduleInput? schedule = null;
            foreach (var property in document.RootElement.EnumerateObject()) {
                if (property.Name == "event") {
                    if (property.Value.ValueKind != JsonValueKind.Object) throw Invalid("event");
                    _ = property.Value.Deserialize<EventInput>(Json);
                    settings = property.Value.Clone();
                } else if (property.Name == "schedule") {
                    if (property.Value.ValueKind != JsonValueKind.Object) throw Invalid("schedule");
                    foreach (var field in new[] { "timezone", "start", "end", "stages", "selection", "presentation" })
                        if (!property.Value.TryGetProperty(field, out _)) throw Invalid("schedule." + field);
                    schedule = ScheduleValidator.Normalize(property.Value.Deserialize<ScheduleInput>(Json)!);
                } else if (property.Name is not ("publication" or "status" or "source" or "timetable" or "operatorRunSheet") ||
                    property.Value.ValueKind != JsonValueKind.String) throw Invalid(property.Name);
            }
            if (settings is null && schedule is null) throw Invalid("$");
            return new(settings, schedule);
        }
        catch (Exception error) when (error is JsonException or DecoderFallbackException or InvalidOperationException) {
            throw Invalid(error is JsonException json ? json.Path ?? "$" : "$");
        }
    }

    private static void CheckDuplicates(JsonElement element, string path)
    {
        if (element.ValueKind == JsonValueKind.Object) {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var property in element.EnumerateObject()) {
                if (!keys.Add(property.Name)) throw Invalid(path + "." + property.Name);
                CheckDuplicates(property.Value, path + "." + property.Name);
            }
        } else if (element.ValueKind == JsonValueKind.Array) {
            var index = 0;
            foreach (var item in element.EnumerateArray()) CheckDuplicates(item, $"{path}[{index++}]");
        }
    }
    private static InputValidationException Invalid(string path) => new(path, "Supply valid UTF-8 seed JSON with distinct, recognized fields and required data objects.");
}
