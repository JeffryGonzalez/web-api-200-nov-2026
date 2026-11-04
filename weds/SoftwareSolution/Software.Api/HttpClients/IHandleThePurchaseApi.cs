namespace Software.Api.HttpClients;

public interface IHandleThePurchaseApi
{
    Task<int> GetApprovedVendorCountAsync(CancellationToken token);
}