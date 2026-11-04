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
