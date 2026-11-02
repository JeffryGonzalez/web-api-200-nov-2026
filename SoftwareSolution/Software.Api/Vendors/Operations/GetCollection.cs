using Marten;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Software.Api.Vendors.Operations;

public static class GetCollection
{
    public static async Task<Ok<VendorSummary>> GetVendorsAsync(
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
}
