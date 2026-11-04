using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Software.Api.Infra;
using Software.Api.Licenses;
using System.Reflection;

namespace Software.Api.OpenApi;

public class HonestSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if(type == typeof(SoftwareVersion))
        {
            schema.Type = JsonSchemaType.String;
            schema.Pattern = @"^\d+\.\d+\.\d+$";
        }
        var atleastOneOf = type.GetCustomAttribute<AtLeastOneOfAttribute>();
        if(atleastOneOf is not null)
        {
            schema.AnyOf = atleastOneOf.PropertyNames.Select(name => RequiredOnly(WireName(context, name))).ToList();
        }
        return Task.CompletedTask;
    }
    private static IOpenApiSchema RequiredOnly(string wireName)
    {
        return new OpenApiSchema { Required = new HashSet<string> { wireName } } ;
    }

    private static string WireName(OpenApiSchemaTransformerContext context, string propertyName)
    {
        return context.JsonTypeInfo.Properties.First(p => (p.AttributeProvider as PropertyInfo)?.Name == propertyName).Name;
    }
}
