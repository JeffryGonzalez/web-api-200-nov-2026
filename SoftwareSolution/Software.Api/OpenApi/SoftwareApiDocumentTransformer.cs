using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Software.Api.OpenApi;

public class SoftwareApiDocumentTransformer(IConfiguration configuration) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info.Title = "Corporate Software Api";
        document.Info.Summary = "An Api created for Web Api 200";

        var authority = configuration["Authentication:Schemes:Bearer:Authority"] ?? "http://localhost:9069/software";

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
                    Scopes = new Dictionary<string, string> { ["openid"] = "Who you are" }
                }
            }
        };

        return Task.CompletedTask;
       
    }
}
