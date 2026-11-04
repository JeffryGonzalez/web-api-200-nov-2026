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
