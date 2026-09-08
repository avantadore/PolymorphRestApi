using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using PolymorphRestApi.ShapeApi.Models;

namespace PolymorphRestApi.ShapeApi;

// Rewrites the flattened anyOf polymorphic schema STJ emits for ShapeBase into a discriminator+allOf
// schema so NSwag generates real C# inheritance (Circle : ShapeBase) instead of flat sibling classes.
internal sealed class ShapeInheritanceSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type != typeof(ShapeBase) || schema.AnyOf is not { Count: > 0 })
        {
            return Task.CompletedTask;
        }

        var mapping = new Dictionary<string, OpenApiSchemaReference>();

        // Each anyOf branch with a "$type" enum property is a real derived variant; the one without it
        // is STJ's FallBackToBaseType branch and should be left as a plain alias of the base.
        foreach (var branch in schema.AnyOf.OfType<OpenApiSchema>())
        {
            if (branch.Properties is not { } props ||
                !props.TryGetValue("$type", out var typeProp) ||
                typeProp is not OpenApiSchema { Enum: [var discriminatorValueNode, ..] })
            {
                continue;
            }

            if (props.Remove("name", out var nameProp) && schema.Properties is null)
            {
                schema.Properties = new Dictionary<string, IOpenApiSchema> { ["name"] = nameProp };
                schema.Required = new HashSet<string> { "name" };
            }

            branch.Required?.Remove("name");

            var ownSchema = new OpenApiSchema
            {
                Type = branch.Type,
                Properties = branch.Properties,
                Required = branch.Required,
            };

            branch.Type = null;
            branch.Properties = null;
            branch.Required = null;
            // Reference the same schema instance being built here for the allOf base (a by-name lookup
            // would re-enter this transformer for ShapeBase itself and stack-overflow); the framework
            // hoists repeated schema instances into components.schemas and rewrites occurrences as $ref.
            branch.AllOf = [schema, ownSchema];
            // Derived types are distinct from ShapeBase, so a by-name reference to them is safe here.
            mapping[discriminatorValueNode!.ToString()] = new OpenApiSchemaReference("ShapeBase" + discriminatorValueNode, context.Document);
        }

        schema.Discriminator = new OpenApiDiscriminator { PropertyName = "$type", Mapping = mapping };

        return Task.CompletedTask;
    }
}
