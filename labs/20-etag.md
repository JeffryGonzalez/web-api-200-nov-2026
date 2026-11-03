# Who changed it first

Protect `DELETE` with an `ETag`, and let `GET` skip sending what a client already has.

> **Builds on the demo and on lab 10.** You need minimal endpoints, an `ETag` on `GET`, and `If-Match`
> on `PUT`. If you followed along, skip the first step.

## Catch up, if you didn't follow along

Do this only if `ReplacePointOfContactAsync` doesn't check `If-Match` yet. Delete `Software.Api/Vendors/Api.cs` if you still have it.

<details>
<summary>Replace Software.Api/Vendors/VendorEndpoints.cs</summary>

```csharp
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

        vendors.MapPost("", AddVendorAsync).RequireAuthorization("SoftwareCenterManager");
        vendors.MapGet("/{id:guid}", GetByIdAsync);
        vendors.MapGet("", GetVendorsAsync);
        vendors.MapDelete("/{id:guid}", DeleteAsync);
        vendors.MapPut("/{id:guid}/point-of-contact", ReplacePointOfContactAsync)
            .RequireAuthorization("SoftwareCenterManager");

        return app;
    }

    private static async Task<Created<VendorDetailsModel>> AddVendorAsync(
        VendorCreateModel request,
        ILogger<VendorEntity> logger, // A static class can't be a type argument, so not ILogger<VendorEndpoints>
        IDocumentSession session,
        ILookupRequestingUsers userLookup,
        TimeProvider clock)
    {
        // We know the person is logged in, a manager, in the software center
        // and the data they passed is "good"
        // Backing Service - database, cache, message broker, etc. NEVER USE THE NEW
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

    // Route Parameter - a "segment" of the URL that holds data.

    private static async Task<Results<Ok<VendorDetailsModel>, NotFound>> GetByIdAsync(
        Guid id,
        IDocumentSession session,
        HttpResponse response)
    {
        var vendor = await session.LoadAsync<VendorEntity>(id);
        if (vendor is null)
        {
            return TypedResults.NotFound(); // 404
        }

        response.Headers.ETag = ETagFor(vendor);
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

    private static async Task<NoContent> DeleteAsync(Guid id, IDocumentSession session)
    {
        // only the person that created it can delete it - otherwise 403
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
```

</details>

<details>
<summary>Replace Software.Api/Vendors/VendorEntity.cs</summary>

```csharp
using Marten.Schema;

namespace Software.Api.Vendors;

public class VendorEntity
{
    public Guid Id { get; set; }
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required VendorPointOfContact PointOfContact { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // Changes on every save. A save only works if this still matches what's in the database.
    [Version] public Guid Version { get; set; }
}
```

</details>

<details>
<summary>Check Software.Api/Program.cs</summary>

These two lines should be there. Add them if they're not. The first goes after `AddControllers`, and the
second goes after `MapControllers`.

```csharp
builder.Services.AddValidation();
```

```csharp
app.MapVendorEndpoints();
```

</details>

<details>
<summary>Add to Software.Api/Software.Api.http</summary>

If it's already there from the starter, replace the old `POST /vendors`.

```http
# @name newVendor
POST {{Software.Api_HostAddress}}/vendors
Authorization: Bearer {{login.response.body.$.access_token}}
Content-Type: application/json

{
  "name": "Hypertheory",
  "url": "https://www.hypertheory.com",
  "pointOfContact": { "name": "Stacey" }
}

###

# @name getVendor
GET {{Software.Api_HostAddress}}{{newVendor.response.headers.Location}}

###

PUT {{Software.Api_HostAddress}}{{newVendor.response.headers.Location}}/point-of-contact
Authorization: Bearer {{login.response.body.$.access_token}}
If-Match: {{getVendor.response.headers.ETag}}
Content-Type: application/json

{
  "name": "Jill",
  "email": "jill@example.com"
}

###
```

</details>

## Send an ETag when a vendor is created

A client that has just created a vendor shouldn't need a `GET` to find out its version. Add the `ETag` header to
the response from `POST`.

<details>
<summary>The code</summary>

Add this parameter to `AddVendorAsync`. The name `response` is already taken in that method.

```csharp
HttpResponse httpResponse
```

Add this just before the `return`:

```csharp
httpResponse.Headers.ETag = ETagFor(entity);
```

</details>

## Don't send what they already have

If a `GET` arrives with `If-None-Match` and that's still the current version, return a 304 with no body. Otherwise,
do what you did before.

<details>
<summary>The code</summary>

```csharp
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
```

</details>

Try it: start the `AppHost`, then add this to your `.http` file. Run `login`, the `POST`, `getVendor`, and then this.

```http
GET {{Software.Api_HostAddress}}{{newVendor.response.headers.Location}}
If-None-Match: {{getVendor.response.headers.ETag}}

###
```

<details>
<summary>What you should see</summary>

A 304, with no body. Now run the `PUT`, then this request again. You should get a 200, because the version has changed.

</details>

## Protect DELETE

`DELETE` is a stub. Make it follow these rules, in this order:

| If | Return |
|---|---|
| No `If-Match` header | 428 |
| The vendor doesn't exist | 404 |
| The caller didn't create it | 403 |
| `If-Match` isn't the current version | 412 |
| Otherwise, delete it | 204 |

Only a Software Center manager can call it.

<details>
<summary>The mapping</summary>

```csharp
vendors.MapDelete("/{id:guid}", DeleteAsync).RequireAuthorization("SoftwareCenterManager");
```

</details>

<details>
<summary>The method</summary>

```csharp
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
```

</details>

## Try it

Add these to your `.http` file. Restart the `AppHost`, then run `login`, the `POST` and `getVendor` again, because the
vendor needs to be new.

```http
# @name loginSue
POST {{Identity_HostAddress}}/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials&client_id=software&client_secret=secret&code=sue

###

# As sue
DELETE {{Software.Api_HostAddress}}{{newVendor.response.headers.Location}}
Authorization: Bearer {{loginSue.response.body.$.access_token}}
If-Match: {{getVendor.response.headers.ETag}}

###

# As mary
DELETE {{Software.Api_HostAddress}}{{newVendor.response.headers.Location}}
Authorization: Bearer {{login.response.body.$.access_token}}
If-Match: {{getVendor.response.headers.ETag}}

###
```

<details>
<summary>What you should see</summary>

- As sue: a 403. Mary created it.
- As mary: a 204.
- As mary, again: a 404. It's gone.

Now run the `POST`, `getVendor` and the `PUT`, then delete as mary. That's a 412, because `getVendor` read a version
that the `PUT` changed.

</details>

## Break it on purpose

`PUT` gets a 412 because the save itself checks the version. So does `DELETE` need its own check?

Comment out the `If-Match` comparison in `DeleteAsync`, the one that returns the 412. Restart, and decide what you expect
before you run this: the `POST`, `getVendor`, the `PUT`, then delete as mary.

<details>
<summary>What you should see</summary>

A 204, and the vendor is gone, even though the version you sent was out of date. Saving a change checks the version, and
this delete doesn't.

</details>

Put the comparison back.

## Optional: write some tests

Write a test that a `PUT` with no `If-Match` gets a 428, and one that shows the second of two writers gets a 412.

<details>
<summary>One way to do it</summary>

Create `Software.Tests/Vendors/ChangingAVendor.cs`:

```csharp
using System.Security.Claims;
using Alba;
using Software.Api.Vendors;

namespace Software.Tests.Vendors;

[Collection("VendorsSystemTests")]
public class ChangingAVendor(VendorsFixture fixture)
{
    private static readonly VendorPointOfContact NewContact = new() { Name = "Jill" };

    private async Task<(string Location, string ETag)> AddVendorAsync()
    {
        var response = await fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.WithClaim(new Claim(ClaimTypes.Role, "Manager"));
            api.Post.Json(new VendorCreateModel
            {
                Name = "Hypertheory",
                Url = "https://www.hypertheory.com",
                PointOfContact = new VendorPointOfContact { Name = "Stacey" }
            }).ToUrl("/vendors");
            api.StatusCodeShouldBe(201);
        });
        var headers = response.Context.Response.Headers;
        return (headers.Location.ToString(), headers.ETag.ToString());
    }

    private Task<IScenarioResult> PutContactAsync(string location, string? ifMatch, int expected)
    {
        return fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.WithClaim(new Claim(ClaimTypes.Role, "Manager"));
            if (ifMatch is not null)
            {
                api.WithRequestHeader("If-Match", ifMatch);
            }
            api.Put.Json(NewContact).ToUrl($"{location}/point-of-contact");
            api.StatusCodeShouldBe(expected);
        });
    }

    [Fact]
    public async Task NoIfMatchGets428()
    {
        var (location, _) = await AddVendorAsync();
        await PutContactAsync(location, null, 428);
    }

    [Fact]
    public async Task TheSecondWriterGets412()
    {
        var (location, etag) = await AddVendorAsync();
        await PutContactAsync(location, etag, 204);
        await PutContactAsync(location, etag, 412);
    }
}
```

</details>

## What you used

- `ETag`, `If-Match` and `If-None-Match` headers
- Status codes 304, 412 and 428
- `[FromHeader(Name = "...")]` on a parameter
- `HttpResponse` as a parameter, and `response.Headers.ETag`
- `TypedResults.StatusCode` and `TypedResults.Forbid`
- A version column, for optimistic concurrency

## Optional: ask your AI

- Write down how you think `ETag` and `If-Match` stop a lost update. Then ask: "Here's my mental model. Where does it break down?"
- Ask: "What's the equivalent of a version column in the database I use at work, and what happens when the check fails?"
