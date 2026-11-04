using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Software.Api.OpenApi;

// The whole document: a title, and how to log in. Scalar reads this to offer a login.
public class SoftwareApiDocumentTransformer(IConfiguration configuration) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        // The same setting the API uses to check tokens, so they can't disagree.
        var authority = configuration["Authentication:Schemes:Bearer:Authority"] ?? "http://localhost:9069/software";

        document.Info.Title = "Software API";
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["oauth2"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{authority}/authorize"),
                    TokenUrl = new Uri($"{authority}/token"),
                    // Scalar only sends a scope the spec lists, and the mock refuses a login without one.
                    Scopes = new Dictionary<string, string> { ["openid"] = "Who you are" }
                }
            }
        };

        return Task.CompletedTask;
    }
}
