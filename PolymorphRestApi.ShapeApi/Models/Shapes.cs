using System.Drawing;
using System.Text.Json.Serialization;

namespace PolymorphRestApi.ShapeApi.Models
{
    [JsonDerivedType(typeof(Circle), typeDiscriminator: "Circle")]
    [JsonDerivedType(typeof(Rectangle), typeDiscriminator: "Rectangle")]
    //[JsonDerivedType(typeof(Triangle), typeDiscriminator: "Triangle")]
    [JsonPolymorphic(
        UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType,
        IgnoreUnrecognizedTypeDiscriminators = true)]
    public class ShapeBase()
    {
        public required string Name { get; set; }
     }


    public class Circle() : ShapeBase
    {
        public required Point Center { get; set; }
        public required int Radius { get; set; }
    }

    public class Rectangle() : ShapeBase
    {
        public required Point TopLeft { get; set; }
        public required int Width { get; set; }
        public required int Height { get; set; }
    }

    // public class Triangle() : ShapeBase
    // {
    //     public required Point Vertex1 { get; set; }
    //     public required Point Vertex2 { get; set; }
    //     public required Point Vertex3 { get; set; }
    // }


    public class Point()
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}