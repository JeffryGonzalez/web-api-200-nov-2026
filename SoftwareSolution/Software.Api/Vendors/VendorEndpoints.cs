using Marten;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Software.Api.Vendors;

public static class VendorEndpoints
{
    // This is the technique in the lab. It's old skool. .NET 10 has a "cooler" way, we will refactor to that.
    public static IEndpointRouteBuilder MapVendorApi(this IEndpointRouteBuilder routes)
    {
        // This creates a "prefix", sor of like 
        var vendors = routes.MapGroup("vendors2");

        vendors.MapPost("", AddVendorAsync);


        return routes;
    }

    private static async Task<Created<VendorDetailsModel>> AddVendorAsync(
        VendorCreateModel request,
        ILogger<Api> logger,
        IDocumentSession session,
        ILookupRequestingUsers userLookup,
        TimeProvider clock
        )
    {
        var entity = new VendorEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Url = request.Url,
            PointOfContact = request.PointOfContact,
            CreatedAt = clock.GetUtcNow(),
            CreatedBy = userLookup.GetRequestingUserId()
        };

        logger.LogInformation("Just added a new vendor {vendor}", entity.Name);
        session.Store(entity);
        await session.SaveChangesAsync();
        var response = new VendorDetailsModel
        {
            Id = entity.Id,
            Name = entity.Name,
            PointOfContact = entity.PointOfContact,
            Url = entity.Url,

        };
        return TypedResults.Created($"/vendors/{entity.Id}", response);
    }
}
