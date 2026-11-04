using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Software.Api.OpenApi;

// One endpoint at a time. It asks the endpoint what it requires, so the spec can't drift from RequireAuthorization.
public class AuthorizeOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var requiresLogin = context.Description.ActionDescriptor.EndpointMetadata.OfType<IAuthorizeData>().Any();
        if (requiresLogin)
        {
            operation.Security =
            [
                new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("oauth2", context.Document)] = [] }
            ];
            operation.Responses ??= new OpenApiResponses();
            operation.Responses["401"] = new OpenApiResponse { Description = "You need to log in." };
            operation.Responses["403"] = new OpenApiResponse { Description = "You aren't allowed to do this." };
        }

        return Task.CompletedTask;
    }
}
