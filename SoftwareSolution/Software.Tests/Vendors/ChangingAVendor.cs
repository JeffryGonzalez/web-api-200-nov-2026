using System.Security.Claims;
using Alba;
using Software.Api.Vendors;

namespace Software.Tests.Vendors;

[Collection("VendorsSystemTests")]
public class ChangingAVendor(VendorsFixture fixture)
{
    private static readonly VendorPointOfContact NewContact = new() { Name = "Jill", Email = "jill@example.com" };

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
                PointOfContact = new VendorPointOfContact { Name = "Stacey", Email = "stacey@example.com" }
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
