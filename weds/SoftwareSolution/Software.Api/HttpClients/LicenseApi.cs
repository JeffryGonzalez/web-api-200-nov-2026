using Common;

namespace Software.Api.HttpClients;


public interface IHandleTheLicenceApi
{
    public Task<Receipt?> SubmitForPayment(string product, int seats);
}
public class LicenseApi(HttpClient httpClient) : IHandleTheLicenceApi
{
    public async Task<Receipt?> SubmitForPayment(string product, int seats)
    {
        var response = await httpClient.PostAsJsonAsync("/requests", value: new LicensePaymentRequest(product, seats));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Receipt?>(); 
    }
}

