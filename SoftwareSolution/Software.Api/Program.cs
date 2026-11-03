using Marten;
using Software.Api.HttpClients;
using Software.Api.Licenses;
using Software.Api.OpenApi;
using Software.Api.Staff;
using Software.Api.Vendors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddValidation();
builder.Services.AddProblemDetails();

builder.AddServiceDefaults(); // ServiceDefaults Extension
builder.Services.AddSingleton<TimeProvider>(sp => TimeProvider.System);
builder.Services.AddScoped<ILookupRequestingUsers, HttpContextRequestingUserLookup>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyOrigin()
.AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("ETag", "Location")));



builder.Services.Configure<RouteOptions>(options =>
    options.SetParameterPolicy<StaffNumberConstraint>("staff-number"));
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

builder.AddNpgsqlDataSource("software");
//var connectionString = builder.Configuration.GetConnectionString("software") ?? throw new Exception("No Connection String");

builder.Services.AddMarten(options =>
{
    //options.Connection(connectionString);
    // add an IDocumentStore to the "services collection", and a "singleton" service to manage connections.

}).UseLightweightSessions()
.UseNpgsqlDataSource();
// appsetting.json

// Add services to the container.


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(config =>
{
    config.AddDocumentTransformer<SoftwareApiDocumentTransformer>();
    config.AddOperationTransformer<AuthorizeOperationTransformer>();
    config.AddSchemaTransformer<HonestSchemaTransformer>();
    
});


builder.Services.AddHttpClient<PurchasingApi>(client =>
{
    client.BaseAddress = new Uri("https+http://purchasing-vendors");
});

// Above this line is host and service configuration - can't do that after you build the app.
var app = builder.Build();
app.UseStatusCodePages(); // Use problem details for stuff like a 404, etc.
app.MapDefaultEndpoints(); // ServiceDefaults - this is mostly health checks.

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // GET /openapi/v1.json 
    app.UseCors();
}

app.UseHttpsRedirection(); // If a request comes in using http, redirect them back to https. Turn off HTTP.

app.UseAuthentication(); // Determining identity
app.UseAuthorization(); // Given an identity, who is allowed to do what?

// This technique is the most common. You CAN (if you are a masochist) write code to register each of your controller endpoints manually.
// nobody* does
app.MapControllers(); // Need to have the builder.Services.AddController() above, this uses reflection to create your routes.
app.MapVendorApi();
app.MapStaffEndpoints();
app.MapLicenseEndpoints();
// GET /status = StatusController Then call GetTheStatus and return to the user-agent whatever that returns.
app.Run(); // It is up and running, waiting for someone to call. 
