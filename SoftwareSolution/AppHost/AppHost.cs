using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

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

var scalar = builder.AddScalarApiReference(options =>
{
    options.PreferHttpsEndpoint = true;
    options.AllowSelfSignedCertificates = true;

}).WithHttpEndpoint(port: 9099, name: "http");


var softwareApi = builder.AddProject<Projects.Software_Api>("software-api")
    .WithReference(softwareDb)
    .WaitFor(softwareDb)
    .WaitFor(identity)
    // Becomes the configuration key Authentication:Schemes:Bearer:Authority in the API.
    .WithEnvironment("Authentication__Schemes__Bearer__Authority",
        () => identity.Resource.GetEndpoint("http").Url + "/software");

scalar.WithApiReference(softwareApi);

builder.Build().Run();
