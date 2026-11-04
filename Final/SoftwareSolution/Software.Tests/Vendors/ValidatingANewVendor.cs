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
