using Bobcat;
using Bobcat.Xunit;
using Software.Api.Vendors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Software.Specs;

[Collection("VendorsSystemTests")]
[BobcatFeature("Adding a new vendor")]
public class AddingANewVendor(VendorsFixture fixture)
{
    [Fact]
    public async Task AddingAVendorReturns()
    {
        await fixture.GivenUser("Sue", "SoftwareCenter", "Manager");
        await fixture.WhenVendorIsPosted(new VendorCreateModel
        {
            Name = "Hypertheory",
            Url = "https://www.hypertheory.com",
            PointOfContact = new VendorPointOfContact
            {
                Name = "Stacey",
                Email = "skg@aol.com"
            }
        });
        await fixture.TheResponseMaps();


    }
    }