using System.Text.Json;
using PolymorphRestApi.ShapeApi.Models;

namespace PolymorphRestApi.ShapeApi.Tests;

public class ShapeSerializationTests
{
    [Fact]
    public void Circle_serialization_contains_valid_type_discriminator_and_round_trips()
    {
        var circle = new Circle
        {
            Name = "Unit circle",
            Center = new Point { X = 10, Y = 20 },
            Radius = 5
        };

        var json = JsonSerializer.Serialize<ShapeBase>(circle);
        using var document = JsonDocument.Parse(json);

        Assert.Equal("Circle", document.RootElement.GetProperty("$type").GetString());

        var deserialized = JsonSerializer.Deserialize<ShapeBase>(json);
        var result = Assert.IsType<Circle>(deserialized);
        Assert.Equal(circle.Name, result.Name);
        Assert.Equal(circle.Center.X, result.Center.X);
        Assert.Equal(circle.Center.Y, result.Center.Y);
        Assert.Equal(circle.Radius, result.Radius);
    }

    [Fact]
    public void Rectangle_serialization_contains_valid_type_discriminator_and_round_trips()
    {
        var rectangle = new Rectangle
        {
            Name = "Unit rectangle",
            TopLeft = new Point { X = 3, Y = 4 },
            Width = 12,
            Height = 8
        };

        var json = JsonSerializer.Serialize<ShapeBase>(rectangle);
        using var document = JsonDocument.Parse(json);

        Assert.Equal("Rectangle", document.RootElement.GetProperty("$type").GetString());

        var deserialized = JsonSerializer.Deserialize<ShapeBase>(json);
        var result = Assert.IsType<Rectangle>(deserialized);
        Assert.Equal(rectangle.Name, result.Name);
        Assert.Equal(rectangle.TopLeft.X, result.TopLeft.X);
        Assert.Equal(rectangle.TopLeft.Y, result.TopLeft.Y);
        Assert.Equal(rectangle.Width, result.Width);
        Assert.Equal(rectangle.Height, result.Height);
    }

    [Fact]
    public void Unknown_shape_json_deserializes_to_shape_base()
    {
        const string json = """
            {
              "$type":"SomeUnknownType",          
              "Name": "Unknown shape",
              "Sides": 7
            }
            """;

        var deserialized = JsonSerializer.Deserialize<ShapeBase>(json);

        var result = Assert.IsType<ShapeBase>(deserialized);
        Assert.Equal("Unknown shape", result.Name);
    }
}
