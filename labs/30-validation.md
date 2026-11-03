# What counts as valid

Let route constraints choose between endpoints, write your own, and turn a validation rule into an attribute.

> **Stands alone.** This works from the starter, or from wherever you are now. There's nothing to catch up on.

## Two routes with one shape

Staff can be looked up by name or by staff number, like `mary` or `S-1042`. There's no database here, just a list.

<details>
<summary>Create Software.Api/Staff/StaffEndpoints.cs</summary>

```csharp
using Microsoft.AspNetCore.Http.HttpResults;

namespace Software.Api.Staff;

public record StaffProfile(string Name, string Number, string Team);

// Just enough to demonstrate route constraints: a hard-coded list and no database.
public static class StaffEndpoints
{
    private static readonly StaffProfile[] Staff =
    [
        new("alice", "S-1007", "Help Desk"),
        new("earl", "S-1019", "Software Center"),
        new("mary", "S-1042", "Software Center"),
        new("sue", "S-1056", "Software Center")
    ];

    public static IEndpointRouteBuilder MapStaffEndpoints(this IEndpointRouteBuilder app)
    {
        var staff = app.MapGroup("/staff");

        staff.MapGet("/{name}", GetByName);
        staff.MapGet("/{number}", GetByNumber);

        return app;
    }

    private static Results<Ok<StaffProfile>, NotFound> GetByName(string name)
    {
        return Found(Staff.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
    }

    private static Results<Ok<StaffProfile>, NotFound> GetByNumber(string number)
    {
        return Found(Staff.FirstOrDefault(s => s.Number == number));
    }

    private static Results<Ok<StaffProfile>, NotFound> Found(StaffProfile? profile)
    {
        return profile is null ? TypedResults.NotFound() : TypedResults.Ok(profile);
    }
}
```

</details>

<details>
<summary>Change Software.Api/Program.cs</summary>

Add this at the top:

```csharp
using Software.Api.Staff;
```

Add this after `app.MapControllers();`:

```csharp
app.MapStaffEndpoints();
```

</details>

Add these to `Software.Api/Software.Api.http`:

```http
GET {{Software.Api_HostAddress}}/staff/mary

###

GET {{Software.Api_HostAddress}}/staff/S-1042

###

GET {{Software.Api_HostAddress}}/staff/S-9999

###

GET {{Software.Api_HostAddress}}/staff/S-12

###
```

Start the `AppHost`, and send the first one. Decide what you expect first.

<details>
<summary>What you should see</summary>

A 500, with this in it:

```
AmbiguousMatchException: The request matched multiple endpoints.
```

Both routes match anything. It builds, and it starts.

</details>

## Use a built-in constraint

Names are only letters. Change the first route.

<details>
<summary>The mapping</summary>

```csharp
staff.MapGet("/{name:alpha}", GetByName);
```

</details>

Restart and send all four. They should give a 200, a 200, a 404 and a 404.

`/staff/S-1042` isn't only letters, so it can't match `alpha`. It works, but `{number}` still accepts anything that isn't
a name. Nothing says what a staff number looks like yet.

## Write your own constraint

A staff number is `S-`, then four digits. Inherit from `RegexRouteConstraint`, and let the compiler generate the regex.

<details>
<summary>Create Software.Api/Staff/StaffNumberConstraint.cs</summary>

```csharp
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing.Constraints;

namespace Software.Api.Staff;

// A staff number looks like S-1042. Registered as "staff-number" in Program.cs.
public partial class StaffNumberConstraint() : RegexRouteConstraint(Pattern())
{
    [GeneratedRegex(@"^S-\d{4}$")]
    private static partial Regex Pattern();
}
```

</details>

<details>
<summary>The mapping</summary>

```csharp
staff.MapGet("/{number:staff-number}", GetByNumber);
```

</details>

## Break it on purpose

Nothing has told routing what `staff-number` means. Before you run it, decide when you expect this to fail. Then restart,
and send a request.

<details>
<summary>What you should see</summary>

It starts. Then every request gets a 500, even `/status`, which has nothing to do with staff:

```
InvalidOperationException: The constraint reference 'staff-number' could not be resolved to a type.
```

</details>

Now register it. Add this after `builder.Services.AddControllers();`:

```csharp
builder.Services.Configure<RouteOptions>(options =>
    options.SetParameterPolicy<StaffNumberConstraint>("staff-number"));
```

## Whose 404 is it?

Restart, and send all four again. Then put a breakpoint in `GetByNumber`, and send the last two.

<details>
<summary>What you should see</summary>

- `/staff/S-9999` reaches your code, finds nobody, and your code returns the 404.
- `/staff/S-12` never reaches your code. Routing found no endpoint, so it's a 404 from nowhere.

A caller can't tell those apart.

</details>

Read the warning at the top of the constraints section in Microsoft's
[routing docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing#route-constraints). Is `S-12` the
kind of input it means? Would you change your answer if someone typed it into a form?

## Look at the spec

Add this to `Software.Api.http`, and send it. Find `/staff/{number}`.

```http
GET {{Software.Api_HostAddress}}/openapi/v1.json

###
```

<details>
<summary>What you should see</summary>

```json
"schema": {
  "pattern": "^S-\\d{4}$",
  "type": "string"
}
```

You didn't ask for that. Anyone reading your spec, or generating a client from it, gets the rule.

</details>

## A validation rule as an attribute

A point of contact needs an email or a phone. Write it as an attribute that names the properties, and put it on the type.

If you followed the demo, `VendorPointOfContact` already has this rule in a `Validate` method. Take out the method and
`: IValidatableObject`.

<details>
<summary>Create Software.Api/Vendors/AtLeastOneOfAttribute.cs</summary>

```csharp
using System.ComponentModel.DataAnnotations;

namespace Software.Api.Vendors;

// The same rule as a Validate method, but written down as data, where other code can read it.
[AttributeUsage(AttributeTargets.Class)]
public class AtLeastOneOfAttribute(params string[] propertyNames) : ValidationAttribute
{
    public string[] PropertyNames { get; } = propertyNames;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var hasOne = PropertyNames.Any(name =>
            !string.IsNullOrWhiteSpace(value?.GetType().GetProperty(name)?.GetValue(value) as string));

        return hasOne
            ? ValidationResult.Success
            : new ValidationResult($"We need at least one of: {string.Join(", ", PropertyNames)}.", PropertyNames);
    }
}
```

</details>

<details>
<summary>Change Software.Api/Vendors/Models.cs</summary>

Add this directly above `public record VendorPointOfContact`:

```csharp
[AtLeastOneOf(nameof(Email), nameof(Phone))]
```

</details>

Add this to `Software.Api.http`. Run `login` first.

```http
# Neither an email nor a phone
POST {{Software.Api_HostAddress}}/vendors
Authorization: Bearer {{login.response.body.$.access_token}}
Content-Type: application/json

{
  "name": "Hypertheory",
  "url": "https://www.hypertheory.com",
  "pointOfContact": { "name": "Stacey" }
}

###
```

You should get a 400 naming both fields. Add a phone, and you should get a 201. Run the tests too. If you did the optional
tests in the ETag lab, their contacts are name-only, so give them an email.

Then send the OpenAPI request again, and find `VendorPointOfContact`. The regex got there by itself. Did this rule?

<details>
<summary>What you should see</summary>

No, there's no sign of it. Attribute or not, nothing has read it yet. Because it's written down as data now, something could.

</details>

## Optional: test the pattern

A pattern has right and wrong answers, so it doesn't need a host, a database or Docker. Write a `[Theory]` for
`StaffNumberConstraint`.

<details>
<summary>One way to do it</summary>

Create `Software.Tests/Staff/StaffNumbers.cs`:

```csharp
using Microsoft.AspNetCore.Routing;
using Software.Api.Staff;

namespace Software.Tests.Staff;

// No host and no database. The pattern is just a thing with right and wrong answers.
public class StaffNumbers
{
    [Theory]
    [InlineData("S-1042", true)]
    [InlineData("S-104", false)]
    [InlineData("S-10422", false)]
    [InlineData("s-1042", false)]
    [InlineData("mary", false)]
    public void MatchesOnlyTheRealPattern(string candidate, bool expected)
    {
        var values = new RouteValueDictionary { ["number"] = candidate };

        var matched = new StaffNumberConstraint()
            .Match(null, null, "number", values, RouteDirection.IncomingRequest);

        Assert.Equal(expected, matched);
    }
}
```

</details>

## Optional: make it real

If you want more practice, implement staff for real. Nothing here tells you how. Some places to start:

- Store them in the database, and add a way to create one.
- Two people can't share a staff number. Where does that rule live? What comes back?
- Send a `Location` and an `ETag`. Make changing a profile safe.
- Who's allowed to read a staff profile? Right now it's anyone.

## What you used

- Built-in route constraints, like `alpha` and `guid`
- `RegexRouteConstraint` and `[GeneratedRegex]`
- `RouteOptions.SetParameterPolicy`
- `AmbiguousMatchException`, again
- A class-level `ValidationAttribute`
- `/openapi/v1.json`

## Optional: ask your AI

If you have an assistant handy, try these. None of them are required.

- Before you ask, decide whether `/staff/S-12` should be a 400 or a 404. Then ask, and see if it can change your mind.
- Paste `AtLeastOneOfAttribute` in and ask: "What could go wrong with this that I wouldn't notice right away?"
