using Bobcat;
using Bobcat.Runtime;
using Software.Api.Vendors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Software.Specs;

[BobcatFeature("Mapping Entities to Models")]
public class EntityMappersTests
{
    [Fact]
    public void CanMap()
    {
        GivenTheEntitiesAre("""
            | Id                                   | Name        | Url                          | CreatedBy |  PointOfContact.Name | PointOfContact.Email | PointOfContact.Phone |
            | d88423c2-3be5-44cb-8adf-d179811be3fd | Hypertheory |  https://www.hypertheory.com | Stacey    | Carl                 | skg@aol.com          | 123-456-7890         |
            | d88423c2-3be5-44cb-8adf-d179811be3fd | Microsoft   |  https://www.microsoft.com   | Carl      | Bill                 | jmg@aol.com          | 123-456-7890         |
            """);

        ThenTheyShouldMapTo("""
            | Id                                   | Name        | Url                          | PointOfContact.Name | PointOfContact.Email | PointOfContact.Phone |
            | d88423c2-3be5-44cb-8adf-d179811be3fd | Hypertheory |  https://www.hypertheory.com | Carl                | skg@aol.com          | 123-456-7890         |
            | d88423c2-3be5-44cb-8adf-d179811be3fd | Microsoft   |  https://www.microsoft.com   | Bill                | jmg@aol.com          | 123-456-7890         |
            """);
    }
    
    private VendorEntity[]? _entities;
    [Given("The entity has")]
    internal void GivenTheEntitiesAre(StepTable entities)
    {
        var rows = TableRunner.BuildRows<VendorEntity>(entities);
        _entities = rows;
    }
    [Then("They Should Map To")]
    internal void ThenTheyShouldMapTo(StepTable expected)
    {
        var rows = TableRunner.BuildRows<VendorDetailsModel>(expected);
        var mapped = _entities!.Select(x => x.MapToVendorsDetails()).ToArray();
        Assert.Equal(rows, mapped);
    }
}