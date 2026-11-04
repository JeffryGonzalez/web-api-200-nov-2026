var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
var app = builder.Build();


// Since vendorId isn't on the route as a route param, it will be found as
// a queryString param - e.g. /catalog?vendorId={someGuid}
app.MapGet("/catalog", (Guid vendorId) =>
{
    var items = new List<CatalogItem>()
    {
        new CatalogItem(Guid.NewGuid(), "Item 1", "Description for Item 1", vendorId),
        new CatalogItem(Guid.NewGuid(), "Item 2", "Description for Item 2", vendorId),
        new CatalogItem(Guid.NewGuid(), "Item 3", "Description for Item 3", vendorId)
    };
    return Results.Ok(items);
});
app.MapDefaultEndpoints();
app.Run();

public record CatalogItem(Guid Id, string Name, string Description,  Guid VendorId);