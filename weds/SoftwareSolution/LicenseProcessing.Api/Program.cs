using Common;
using System.Numerics;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints(); // health checks, otel, etc.

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapPost("/requests", async (LicensePaymentRequest request) => {
    var total = 0.0M;
    foreach(var seat in Enumerable.Range(1, request.Seats))
{
    await Task.Delay(300);
    total += 99.0M;
}   

    return new Receipt(request.Product, request.Seats, total);
});


app.Run();

