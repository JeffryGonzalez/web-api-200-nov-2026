using JasperFx;
using Marten;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Software.Api.Vendors.Operations;

namespace Software.Api.Vendors;

public static class VendorEndpoints
{
    // This is the technique in the lab. It's old skool. .NET 10 has a "cooler" way, we will refactor to that.
    public static IEndpointRouteBuilder MapVendorApi(this IEndpointRouteBuilder routes)
    {
        // This creates a "prefix", sor of like 
        var vendors = routes.MapGroup("vendors").WithDescription("Vendor Management").WithTags("Vendor Lookups");
        var vendorManagersEndpoints = routes.MapGroup("vendors").RequireAuthorization("SoftwareCenterManager")
            .WithDescription("Software center manager endpoints")
            .WithTags("Vendor Management");

        vendors.MapGet("/{id:guid}", GetByIdAsync);
        vendors.MapGet("", GetCollection.GetVendorsAsync);

        vendorManagersEndpoints.MapPost("", AddVendorAsync);
        vendorManagersEndpoints.MapDelete("/{id:guid}", DeleteAsync);
        vendorManagersEndpoints.MapPut("/{id:guid}/point-of-contact", ReplacePointOfContactAsync);
        return routes;
    }

    private static async Task<Results<NoContent, StatusCodeHttpResult>> DeleteAsync(Guid id, IDocumentSession session,
        [FromHeader(Name ="If-Match")] string? ifMatch,
        ILookupRequestingUsers userLookups
        )
    {

        if(ifMatch is null)
        {
            return TypedResults.StatusCode(StatusCodes.Status428PreconditionRequired);
        }
        // only the person that created it can delete it - otherwise 403
        var vendor = await session.LoadAsync<VendorEntity>(id);

        if (ifMatch.Trim('"') != ETagFor(vendor))
        {
            return TypedResults.StatusCode(StatusCodes.Status412PreconditionFailed);
        }
        session.Delete(vendor);
        await session.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, StatusCodeHttpResult, NotFound>> ReplacePointOfContactAsync(
        Guid id,
        IDocumentSession session,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        VendorPointOfContact request)
    {
        if (ifMatch is null)
        {
            return TypedResults.StatusCode(StatusCodes.Status428PreconditionRequired);

        }
        // added a header, but wrong format
        if (!Guid.TryParse(ifMatch.Trim('"'), out var version))
        {
            return TypedResults.StatusCode(StatusCodes.Status412PreconditionFailed);
        }
        // if the vendor does not exist, return 404
        var vendor = await session.LoadAsync<VendorEntity>(id);
        if (vendor is null)
        {
            return TypedResults.NotFound();
        }
    
        vendor.PointOfContact = request;
        vendor.Version = version;
        session.Store(vendor);
        // Somone else modified it so the version doesn't match, so punch them in the nose.
        try
        {
            await session.SaveChangesAsync();
        }
        catch (ConcurrencyException)
        {
            return TypedResults.StatusCode(StatusCodes.Status412PreconditionFailed);
        }
        return TypedResults.NoContent();
    }
    private static async Task<Created<VendorDetailsModel>> AddVendorAsync(
        VendorCreateModel request,
        ILogger<VendorEntity> logger,
        IDocumentSession session,
        ILookupRequestingUsers userLookup,
        HttpResponse httpResponse,
        TimeProvider clock
        )
    {


        var entity = request.MapToEntity(clock.GetUtcNow(), userLookup.GetRequestingUserId());
        logger.LogInformation("Just added a new vendor {vendor}", entity.Name);
        session.Store(entity);
        await session.SaveChangesAsync();

        var response = entity.MapToVendorsDetails();
        httpResponse.Headers.ETag = ETagFor(entity);
        return TypedResults.Created($"/vendors/{entity.Id}", response);
    }

    // We are going to ask them for an e-tag if they have a memory of this,
    // if it is the same as we gave them before, tell them "cool", otherwise, send them the new version.

    private static async Task<Results<Ok<VendorDetailsModel>, NotFound, StatusCodeHttpResult>> GetByIdAsync(
    Guid id,
    [FromHeader(Name = "If-None-Match")] string? ifNoneMatch,
    HttpResponse httpResponse,
    IDocumentSession session)
    {
        var saved = await session.Query<VendorEntity>()
            .Where(v => v.Id == id)

            .SingleOrDefaultAsync();

        if (saved is null)
        {
            return TypedResults.NotFound();
        }

        if (ifNoneMatch == ETagFor(saved))
        {
            return TypedResults.StatusCode(StatusCodes.Status304NotModified); // Yours is good, yo! cool.
        }
        httpResponse.Headers.ETag = ETagFor(saved);

        var response = saved.MapToVendorsDetails();
        return saved switch
        {
            null => TypedResults.NotFound(), // 404
            _ => TypedResults.Ok(response) // 200 - with the data.
        };
    }

    private static string ETagFor(VendorEntity vendor) => $"{vendor.Version}";

}
