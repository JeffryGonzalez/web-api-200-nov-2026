using Common;

namespace Software.Api.HttpClients;

public class LicenseApi(HttpClient httpClient)
{
    internal async Task<Receipt?> SubmitForPayment(string product, int seats)
    {
        var response = await httpClient.PostAsJsonAsync("/requests", value: new LicensePaymentRequest(product, seats));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Receipt?>(); 
    }
}

