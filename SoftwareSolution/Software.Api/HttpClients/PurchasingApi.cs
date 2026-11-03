namespace Software.Api.HttpClients;

public class PurchasingApi(HttpClient client)
{

    // everything we do with that API happens here. ALL OF IT.
    // Anything we do with other APIs (different origins) DOES NOT HAPPEN HERE
    // One typed http client per origin

    public async Task<int> GetApprovedVendorCountAsync(CancellationToken token)
    {
        // go through all the retries, if that doesn't work, add a circuit breaker for X seconds
        var response = await client.GetAsync("clerk-records/purchasing/vendors?status=approved", token);

        response.EnsureSuccessStatusCode(); // This is you asking to be punched in the nose if the response isn't 200-299
        // it throws an exception

        var itemsPage = await response.Content.ReadFromJsonAsync<CatalogPageItem>();

        return itemsPage?.Items?.Count ?? throw new Exception("no count property, whatever...");

        
    }
}


// A form of quiet quitting 

public record CatalogPageItem
{
    public List<object>? Items { get; set; }

}