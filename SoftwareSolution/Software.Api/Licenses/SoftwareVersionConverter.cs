using System.Text.Json;
using System.Text.Json.Serialization;

namespace Software.Api.Licenses;

// On the wire a version is "2026.1.3", not an object with three numbers.
public class SoftwareVersionConverter : JsonConverter<SoftwareVersion>
{
    public override SoftwareVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var parts = reader.GetString()?.Split('.') ?? [];
        if (parts.Length == 3
            && int.TryParse(parts[0], out var major)
            && int.TryParse(parts[1], out var minor)
            && int.TryParse(parts[2], out var patch))
        {
            return new SoftwareVersion(major, minor, patch);
        }

        throw new JsonException("A version looks like 2026.1.3.");
    }

    public override void Write(Utf8JsonWriter writer, SoftwareVersion value, JsonSerializerOptions options)
    {
        writer.WriteStringValue($"{value.Major}.{value.Minor}.{value.Patch}");
    }
}
