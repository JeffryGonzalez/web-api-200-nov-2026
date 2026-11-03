using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Routing.Constraints;

namespace Software.Api.Staff;

// A staff number looks like S-1042. Registered as "staff-number" in Program.cs.
public partial class StaffNumberConstraint() : RegexRouteConstraint(Pattern())
{
    [GeneratedRegex(@"^S-\d{4}$")]
    private static partial Regex Pattern();
}