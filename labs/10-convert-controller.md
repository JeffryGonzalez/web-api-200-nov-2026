# The rest of the Vendors API

Finish converting the Vendors API from controllers to minimal endpoints.

> **Builds on the demo.** You need `POST /vendors` already converted. If you followed along,
> skip the first step.

## Catch up, if you didn't follow along

Do this only if `Software.Api/Vendors/VendorEndpoints.cs` doesn't exist yet.

<details>
<summary>Create Software.Api/Vendors/VendorEndpoints.cs</summary>

```csharp
using Marten;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Software.Api.Vendors;

public static class VendorEndpoints
{
    public static IEndpointRouteBuilder MapVendorEndpoints(this IEndpointRouteBuilder app)
    {
        var vendors = app.MapGroup("/vendors");

        vendors.MapPost("", AddVendorAsync).RequireAuthorization("SoftwareCenterManager");

        return app;
    }

    private static async Task<Created<VendorDetailsModel>> AddVendorAsync(
        VendorCreateModel request,
        ILogger<VendorEntity> logger, // A static class can't be a type argument, so not ILogger<VendorEndpoints>
        IDocumentSession session,
        ILookupRequestingUsers userLookup,
        TimeProvider clock)
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
```

</details>

<details>
<summary>Change Software.Api/Program.cs</summary>

Add this after `builder.Services.AddControllers();`:

```csharp
builder.Services.AddValidation();
```

Add this after `app.MapControllers();`:

```csharp
app.MapVendorEndpoints();
```

</details>

Then delete `AddVendorAsync` from `Software.Api/Vendors/Api.cs`, including its `[HttpPost]` and
`[Authorize]` attributes.

<details>
<summary>Create Software.Tests/Vendors/ValidatingANewVendor.cs</summary>

```csharp
using System.Security.Claims;
using Alba;
using Software.Api.Vendors;

namespace Software.Tests.Vendors;

[Collection("VendorsSystemTests")]
public class ValidatingANewVendor(VendorsFixture fixture)
{
    [Fact]
    public async Task BadDataGets400()
    {
        var badVendor = new VendorCreateModel
        {
            Name = "abc", // too short
            Url = "not a url",
            PointOfContact = new VendorPointOfContact { Name = "Stacey" }
        };

        await fixture.Host.Scenario(api =>
        {
            api.WithClaim(new Claim("sub", "jill@company.com"));
            api.WithClaim(new Claim(ClaimTypes.Role, "SoftwareCenter"));
            api.WithClaim(new Claim(ClaimTypes.Role, "Manager"));
            api.Post.Json(badVendor).ToUrl("/vendors");
            api.StatusCodeShouldBe(400);
        });
    }
}
```

</details>

Run the tests. They should all pass.

## Get a vendor by id

Move `GetByIdAsync` from `Api.cs` to `VendorEndpoints.cs`, and map it to `GET /vendors/{id}`. It
can return a 200 or a 404, and its return type should say so.

Don't delete the one in `Api.cs` yet.

<details>
<summary>The mapping</summary>

```csharp
vendors.MapGet("/{id:guid}", GetByIdAsync);
```

</details>

<details>
<summary>The method</summary>

```csharp
private static async Task<Results<Ok<VendorDetailsModel>, NotFound>> GetByIdAsync(
    Guid id,
    IDocumentSession session)
{
    var saved = await session.Query<VendorEntity>()
        .Where(v => v.Id == id)
        .Select(v => new VendorDetailsModel
        {
            Name = v.Name,
            PointOfContact = v.PointOfContact,
            Url = v.Url,
            Id = v.Id
        })
        .SingleOrDefaultAsync();

    return saved switch
    {
        null => TypedResults.NotFound(), // 404
        _ => TypedResults.Ok(saved) // 200 - with the data.
    };
}
```

</details>

## Break it on purpose

Both versions of `GetByIdAsync` are in place now. Before you run the tests, decide what you expect
to happen. Then run them.

<details>
<summary>What you should see</summary>

`AddingAVendor` fails, because its `GET` returns a 500 instead of a 200. Look for this in the output:

```
AmbiguousMatchException: The request matched multiple endpoints.
```

It builds, and it starts. It only fails when a request arrives.

</details>

Now delete `GetByIdAsync` from `Api.cs` and run the tests again. They should all pass.

## Get the list of vendors

Move `GetVendorsAsync` to `VendorEndpoints.cs`, and map it to `GET /vendors`. Delete it from `Api.cs`.

Minimal APIs figure out where parameters come from, so `[FromQuery]` and `[FromServices]` can go.

<details>
<summary>The mapping</summary>

```csharp
vendors.MapGet("", GetVendorsAsync);
```

</details>

<details>
<summary>The method</summary>

Optional parameters have to come last, so `token` moves ahead of `page` and `pageSize`.

```csharp
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
```

</details>

## Delete and replace

Move `DeleteAsync` and `ReplacePointOfContactAsync`, and map them. They're still stubs that return
a 204, and they stay that way for now. Keep their comments.

Then delete `Api.cs`. `/status` is still a controller, so `AddControllers` and `MapControllers`
stay in `Program.cs`.

<details>
<summary>The mappings</summary>

```csharp
vendors.MapDelete("/{id:guid}", DeleteAsync);
vendors.MapPut("/{id:guid}/point-of-contact", ReplacePointOfContactAsync);
```

</details>

<details>
<summary>The methods</summary>

```csharp
private static async Task<NoContent> DeleteAsync(Guid id, IDocumentSession session)
{
    // only the person that created it can delete it - otherwise 403
    return TypedResults.NoContent();
}

private static async Task<NoContent> ReplacePointOfContactAsync(
    Guid id,
    IDocumentSession session,
    VendorPointOfContact request)
{
    // if the vendor does not exist, return 404
    //
    return TypedResults.NoContent();
}
```

</details>

Run the tests. They should all pass.

## Write a test

Nothing tests the 404 yet. In `Software.Tests/Vendors/`, write a test showing that a `GET` for a
vendor that doesn't exist gets a 404.

<details>
<summary>One way to do it</summary>

Create `Software.Tests/Vendors/GettingAVendor.cs`:

```csharp
using Alba;

namespace Software.Tests.Vendors;

[Collection("VendorsSystemTests")]
public class GettingAVendor(VendorsFixture fixture)
{
    [Fact]
    public async Task AVendorThatDoesNotExistGets404()
    {
        await fixture.Host.Scenario(api =>
        {
            api.Get.Url($"/vendors/{Guid.NewGuid()}");
            api.StatusCodeShouldBe(404);
        });
    }
}
```

</details>

## Try it for real

Start the `AppHost`, and wait for the dashboard to show `software-api` running.

Add these to `Software.Api/Software.Api.http`. Run the `login` request first, then each of these.

```http
GET {{Software.Api_HostAddress}}/vendors?page=1&pageSize=5

###

GET {{Software.Api_HostAddress}}/vendors?page=abc

###

GET {{Software.Api_HostAddress}}/status

###

# Notice what's missing
POST {{Software.Api_HostAddress}}/vendors
Authorization: Bearer {{login.response.body.$.access_token}}
Content-Type: application/json

{
  "name": "Hypertheory",
  "pointOfContact": { "name": "Stacey" }
}

###
```

<details>
<summary>What you should see</summary>

- The list returns a 200. It's empty if you haven't added a vendor.
- `page=abc` returns a 400. Nothing in your code checked that.
- `/status` returns a 200 from the controller, running beside your endpoints.
- The `POST` with no `url` returns a 400. Look at the body.

</details>

## What you used

- `app.MapGroup`, `MapGet`, `MapPost`, `MapPut`, `MapDelete`
- `RequireAuthorization` with a policy name
- `TypedResults`, and `Results<T1, T2>` as a return type
- Parameter inference: route, query string, body and services
- `builder.Services.AddValidation()`
- `AmbiguousMatchException`
- Testing endpoints with Alba's `Scenario`

## Optional: ask your AI

If you have an assistant handy, try these. None of them are required.

- Paste `VendorEndpoints.cs` in and ask: "What subtle problems could bite me here that aren't obvious
  right now?"
- Before you ask, write down what you think changed for a caller of this API when it moved off
  controllers. Then ask, and compare.
