using Microsoft.AspNetCore.Mvc;

namespace Software.Api.Infra;

public class StatusController : ControllerBase
{

    // GET /status
    [HttpGet("/status")]
    public async Task<ActionResult> GetTheStatus()
    {
        var response = new StatusResponseMessage
        {
            Status = "Awesome, all system go!",
            Checked = DateTimeOffset.UtcNow,
            Level = StatusLevel.Green,
        };
        return Ok(response);
    }
}


public enum StatusLevel { Green, Yellow, Red }

public record StatusResponseMessage
{
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset Checked { get; set; }
    public StatusLevel Level { get; set; }
}