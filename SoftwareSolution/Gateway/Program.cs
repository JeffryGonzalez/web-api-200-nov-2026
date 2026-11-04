var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();


builder.Services.AddReverseProxy()
    .AddServiceDiscoveryDestinationResolver() // service discovery (e.g. "https+http://some-service")
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();
app.MapDefaultEndpoints();
app.Run();