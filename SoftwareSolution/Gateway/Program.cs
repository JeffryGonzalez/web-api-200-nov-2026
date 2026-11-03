var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddReverseProxy()
    .AddServiceDiscoveryDestinationResolver()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapReverseProxy();
app.MapDefaultEndpoints();
app.Run();