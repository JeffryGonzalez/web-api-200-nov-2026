using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

// I don't like this usually - I'd use this as a stepping stone, much better to have a "responder" (but local)
var externalApi = builder.AddExternalService("purchasing-vendors", "https://theoria.hypertheory-labs.com");
var pgServer = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithImageTag("17"); // make this as close to version you are using in prod as you can (dev/prod parity);

var softwareDb = pgServer.AddDatabase("software");

// A mock OAuth2 server that pretends to be an identity provider. Great for development.
var identity = builder.AddContainer("identity", "ghcr.io/navikt/mock-oauth2-server:3.0.1")
    .WithHttpEndpoint(9069, 8080)
    .WithBindMount("./MockOauth2/", "/app/resources/software/") // the users and the login page
    .WithEnvironment("JSON_CONFIG_PATH", "/app/resources/software/settings/config.json")
    .WithLifetime(ContainerLifetime.Persistent);

// TODO: Show the endpoint url
var scalar = builder.AddScalarApiReference(options =>
{
    options.PreferHttpsEndpoint = true;
    options.AllowSelfSignedCertificates = true;
    options.DisableDefaultProxy();
    options.AddPreferredSecuritySchemes("oauth2")
        .AddAuthorizationCodeFlow("oauth2", flow => flow.WithClientId("software").WithClientSecret("secret").WithSelectedScopes(["openid"]));
        
    

}).WithHttpEndpoint(port: 9099, name: "http");
var licenseApi = builder.AddProject<Projects.LicenseProcessing_Api>("licenseprocessing-api");

var softwareApi = builder.AddProject<Projects.Software_Api>("software-api")
    .WithReference(softwareDb)
    .WithReference(externalApi)
    .WaitFor(softwareDb)
    .WithReference(licenseApi)
    .WaitFor(licenseApi)
    .WaitFor(identity)
    // Becomes the configuration key Authentication:Schemes:Bearer:Authority in the API.
    .WithEnvironment("Authentication__Schemes__Bearer__Authority",
        () => identity.Resource.GetEndpoint("http").Url + "/software");

scalar.WithApiReference(softwareApi);



builder.Build().Run();
