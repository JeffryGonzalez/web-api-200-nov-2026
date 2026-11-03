using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Software.Api.Infra;
using System.Reflection;

namespace Software.Api.OpenApi;

public class HonestSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
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
