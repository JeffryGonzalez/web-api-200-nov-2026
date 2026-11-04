using Marten;
using System.Text.Json.Serialization;
using Software.Api.Licenses;
using Software.Api.OpenApi;
using Software.Api.Staff;
using Software.Api.Vendors;

var builder = WebApplication.CreateBuilder(args);

// Controllers and minimal APIs each have their own JSON options. Pick one, if you can.
builder.Services.AddControllers().AddJsonOptions(options =>
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow; // A typo is a 400.
});
builder.Services.AddValidation(); // For minimal APIs. [ApiController] did this for controllers.
builder.Services.AddProblemDetails(); // One format for errors: problem+json, RFC 9457.
builder.Services.Configure<RouteOptions>(options =>
    options.SetParameterPolicy<StaffNumberConstraint>("staff-number")); // {number:staff-number}
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
    .WithExposedHeaders("ETag", "Location"))); // Scalar is a browser, and scripts can't read these otherwise.
builder.AddServiceDefaults(); // ServiceDefaults Extension
builder.Services.AddSingleton<TimeProvider>(sp => TimeProvider.System);
builder.Services.AddScoped<ILookupRequestingUsers, HttpContextRequestingUserLookup>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    // Keep the token's own claim names ("sub", "role"), instead of mapping them to long URIs.
    options.MapInboundClaims = false;
    options.TokenValidationParameters.NameClaimType = "sub"; // What User.Identity.Name reads
    options.TokenValidationParameters.RoleClaimType = "role"; // What RequireRole checks
});

builder.Services.AddAuthorizationBuilder().AddPolicy("SoftwareCenterManager", policy =>
{
    policy.RequireRole("SoftwareCenter");
    policy.RequireRole("Manager");
});

var connectionString = builder.Configuration.GetConnectionString("software") ?? throw new Exception("No Connection String");

builder.Services.AddMarten(options =>
{
    options.Connection(connectionString);
    // add an IDocumentStore to the "services collection", and a "singleton" service to manage connections.

}).UseLightweightSessions();
// appsetting.json

// Add services to the container.


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(config =>
{
    config.AddDocumentTransformer<SoftwareApiDocumentTransformer>();
    config.AddOperationTransformer<AuthorizeOperationTransformer>();
    config.AddSchemaTransformer<HonestSchemaTransformer>();
});
    

// Above this line is host and service configuration - can't do that after you build the app.
var app = builder.Build();
app.UseStatusCodePages(); // An error with no body gets a problem+json body.
app.MapDefaultEndpoints(); // ServiceDefaults - this is mostly health checks.

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // GET /openapi/v1.json
    app.UseCors(); // Development only: Scalar is on another origin.
}

app.UseHttpsRedirection(); // If a request comes in using http, redirect them back to https. Turn off HTTP.

app.UseAuthentication(); // Determining identity
app.UseAuthorization(); // Given an identity, who is allowed to do what?

app.MapControllers(); // Need to have the builder.Services.AddController() above, this uses reflection to create your routes.
app.MapVendorEndpoints();
app.MapStaffEndpoints();
app.MapLicenseEndpoints();
// GET /status = StatusController Then call GetTheStatus and return to the user-agent whatever that returns.
app.Run(); // It is up and running, waiting for someone to call. 
