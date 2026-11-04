using Microsoft.AspNetCore.Mvc;
using Software.Api.HttpClients;

namespace Software.Api.Infra;

[ApiController]
public class StatusController(IHandleThePurchaseApi apiClient) : ControllerBase
{

    // GET /status
    [HttpGet("/status")]
    public async Task<ActionResult> GetTheStatus(CancellationToken token)
    {
        // Chaos Monkey stuff - If the other API fails - and it will - what are you going to do?
        int count = -1;
        try
        {
            count = await apiClient.GetApprovedVendorCountAsync(token);
        }
        catch (Exception)
        {

           
        }
      
        var response = new StatusResponseMessage
        {
            Status = "Awesome, all system go!",
            Checked = DateTimeOffset.UtcNow,
            NumberOfVendors = count
        };
        return Ok(response);
    }
}


public record StatusResponseMessage
{
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset Checked { get; set; }
    public int NumberOfVendors { get; set; }
}