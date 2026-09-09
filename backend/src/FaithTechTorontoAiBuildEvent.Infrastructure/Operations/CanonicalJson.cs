using System.Security.Cryptography;
using System.Text.Json;
namespace FaithTechTorontoAiBuildEvent.Infrastructure.Operations;

public static class CanonicalJson
{
    public static string Hash<T>(T value)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream)) Write(writer, JsonSerializer.SerializeToElement(value));
        return Convert.ToHexString(SHA256.HashData(stream.ToArray()));
    }
    private static void Write(Utf8JsonWriter writer, JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object) {
            writer.WriteStartObject();
            foreach (var property in element.EnumerateObject().OrderBy(x => x.Name, StringComparer.Ordinal)) {
                writer.WritePropertyName(property.Name); Write(writer, property.Value);
            }
            writer.WriteEndObject();
        } else if (element.ValueKind == JsonValueKind.Array) {
            writer.WriteStartArray(); foreach (var item in element.EnumerateArray()) Write(writer, item); writer.WriteEndArray();
        } else element.WriteTo(writer);
    }
}
