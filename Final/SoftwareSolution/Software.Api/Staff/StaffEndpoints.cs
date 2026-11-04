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

        staff.MapGet("/{name:alpha}", GetByName);
        staff.MapGet("/{number:staff-number}", GetByNumber);

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
