using JasperFx;
using Marten;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Software.Api.Vendors;

public static class VendorEndpoints
{
    public static IEndpointRouteBuilder MapVendorEndpoints(this IEndpointRouteBuilder app)
    {
        var vendors = app.MapGroup("/vendors");

        vendors.MapPost("", AddVendorAsync)
            .RequireAuthorization("SoftwareCenterManager")
            .ProducesValidationProblem();
        vendors.MapGet("/{id:guid}", GetByIdAsync).WithName("GetVendorById");
        vendors.MapGet("", GetVendorsAsync);
        vendors.MapDelete("/{id:guid}", DeleteAsync).RequireAuthorization("SoftwareCenterManager");
        vendors.MapPut("/{id:guid}/point-of-contact", ReplacePointOfContactAsync)
            .RequireAuthorization("SoftwareCenterManager");

        return app;
    }

    private static async Task<Created<VendorDetailsModel>> AddVendorAsync(
        VendorCreateModel request,
        [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey,
        ILogger<VendorEntity> logger, // A static class can't be a type argument, so not ILogger<VendorEndpoints>
        IDocumentSession session,
        ILookupRequestingUsers userLookup,
        TimeProvider clock,
        HttpResponse httpResponse,
        LinkGenerator links)
    {
        // We know the person is logged in, a manager, in the software center
        // and the data they passed is "good"
        // Backing Service - database, cache, message broker, etc. NEVER USE THE NEW
        var entity = new VendorEntity
        {
            Id = idempotencyKey ?? Guid.NewGuid(),
            Name = request.Name,
            Url = request.Url,
            PointOfContact = request.PointOfContact,
            CreatedAt = clock.GetUtcNow(),
            CreatedBy = userLookup.GetRequestingUserId()
        };

        logger.LogInformation("Just added a new vendor {vendor}", entity.Name);
        session.Insert(entity);
        try
        {
            await session.SaveChangesAsync();
        }
        catch (DocumentAlreadyExistsException)
        {
            // We've seen this key. Respond as though we did the work again.
            entity = (await session.LoadAsync<VendorEntity>(entity.Id))!;
        }

        var response = new VendorDetailsModel
        {
            Id = entity.Id,
            Name = entity.Name,
            PointOfContact = entity.PointOfContact,
            Url = entity.Url,
        };
        httpResponse.Headers.ETag = ETagFor(entity);
        return TypedResults.Created(links.GetPathByName("GetVendorById", new { id = entity.Id }), response);
    }

    // Route Parameter - a "segment" of the URL that holds data.

    private static async Task<Results<Ok<VendorDetailsModel>, NotFound, StatusCodeHttpResult>> GetByIdAsync(
        Guid id,
        [FromHeader(Name = "If-None-Match")] string? ifNoneMatch,
        IDocumentSession session,
        HttpResponse response)
    {
        var vendor = await session.LoadAsync<VendorEntity>(id);
        if (vendor is null)
        {
            return TypedResults.NotFound(); // 404
        }

        response.Headers.ETag = ETagFor(vendor);
        if (ifNoneMatch == ETagFor(vendor))
        {
            return TypedResults.StatusCode(StatusCodes.Status304NotModified); // You already have it.
        }

        return TypedResults.Ok(new VendorDetailsModel // 200 - with the data.
        {
            Id = vendor.Id,
            Name = vendor.Name,
            PointOfContact = vendor.PointOfContact,
            Url = vendor.Url
        });
    }

    private static async Task<Ok<VendorSummary>> GetVendorsAsync(
        IDocumentSession session,
        CancellationToken token,
        int page = 1,
        int pageSize = 10)
    {
        var results = await session.Query<VendorEntity>()
            .OrderBy(v => v.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VendorSummaryModel
            {
                Id = v.Id,
                Name = v.Name,
                Url = v.Url
            })
            .ToListAsync(token);

        var response = new VendorSummary
        {
            Vendors = results,
            Page = page,
            PageSize = pageSize
        };
        return TypedResults.Ok(response);
    }

    private static async Task<Results<NoContent, NotFound, ForbidHttpResult, StatusCodeHttpResult>> DeleteAsync(
        Guid id,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        IDocumentSession session,
        ILookupRequestingUsers userLookup)
    {
        if (ifMatch is null)
        {
            return TypedResults.StatusCode(StatusCodes.Status428PreconditionRequired);
        }

        var vendor = await session.LoadAsync<VendorEntity>(id);
        if (vendor is null)
        {
            return TypedResults.NotFound();
        }

        // only the person that created it can delete it - otherwise 403
        if (vendor.CreatedBy != userLookup.GetRequestingUserId())
        {
            return TypedResults.Forbid();
        }

        if (ifMatch != ETagFor(vendor))
        {
            return TypedResults.StatusCode(StatusCodes.Status412PreconditionFailed);
        }

        session.Delete(vendor);
        await session.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound, StatusCodeHttpResult>> ReplacePointOfContactAsync(
        Guid id,
        VendorPointOfContact request,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        IDocumentSession session,
        HttpResponse response)
    {
        if (ifMatch is null)
        {
            return TypedResults.StatusCode(StatusCodes.Status428PreconditionRequired);
        }

        var vendor = await session.LoadAsync<VendorEntity>(id);
        if (vendor is null)
        {
            return TypedResults.NotFound();
        }

        if (!Guid.TryParse(ifMatch.Trim('"'), out var version))
        {
            return TypedResults.StatusCode(StatusCodes.Status412PreconditionFailed);
        }

        vendor.PointOfContact = request;
        vendor.Version = version; // "Only save this if the version is still the one the client read."
        session.Store(vendor);
        try
        {
            await session.SaveChangesAsync();
        }
        catch (ConcurrencyException)
        {
            return TypedResults.StatusCode(StatusCodes.Status412PreconditionFailed);
        }

        response.Headers.ETag = ETagFor(vendor);
        return TypedResults.NoContent();
    }

    private static string ETagFor(VendorEntity vendor) => $"\"{vendor.Version}\"";
}
