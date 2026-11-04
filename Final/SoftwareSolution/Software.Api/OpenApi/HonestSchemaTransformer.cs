using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Software.Api.Licenses;
using Software.Api.Vendors;

namespace Software.Api.OpenApi;

// The generator can't see inside a converter or a rule, so this tells it what they do.
public class HonestSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if (type == typeof(SoftwareVersion))
        {
            schema.Type = JsonSchemaType.String;
            schema.Pattern = @"^\d+\.\d+\.\d+$";
        }

        var atLeastOneOf = type.GetCustomAttribute<AtLeastOneOfAttribute>();
        if (atLeastOneOf is not null)
        {
            schema.AnyOf = atLeastOneOf.PropertyNames
                .Select(name => RequiredOnly(WireName(context, name)))
                .ToList();
        }

        return Task.CompletedTask;
    }

    // What the property is called in JSON, whatever a naming policy or [JsonPropertyName] did to it.
    private static string WireName(OpenApiSchemaTransformerContext context, string propertyName)
    {
        return context.JsonTypeInfo.Properties
            .First(p => (p.AttributeProvider as PropertyInfo)?.Name == propertyName)
            .Name;
    }

    private static IOpenApiSchema RequiredOnly(string wireName)
    {
        return new OpenApiSchema { Required = new HashSet<string> { wireName } };
    }
}
