using System.Text.Json;
using System.Text.Json.Serialization;
using PolymorpTestClient.Models;

namespace PolymorphTestClient;

// NSwag's generated JsonInheritanceConverter throws on a "$type" it doesn't know (e.g. a Triangle added
// server-side after this client's model was generated). Registering this converter in JsonSerializerOptions
// takes priority over that type-level attribute converter, letting unknown shapes fall back to ShapeBase.
internal sealed class ShapeBaseFallbackConverter : JsonConverter<ShapeBase>
{
    public override ShapeBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;
        var type = root.TryGetProperty("$type", out var typeProperty) ? typeProperty.GetString() : null;

        return type switch
        {
            "Circle" => root.Deserialize<ShapeBaseCircle>(options),
            "Rectangle" => root.Deserialize<ShapeBaseRectangle>(options),
            _ => new ShapeBase
            {
                Name = root.TryGetProperty("name", out var nameProperty) ? nameProperty.GetString() ?? string.Empty : string.Empty
            }
        };
    }

    public override void Write(Utf8JsonWriter writer, ShapeBase value, JsonSerializerOptions options)
        => JsonSerializer.Serialize(writer, value, value.GetType(), options);
}
