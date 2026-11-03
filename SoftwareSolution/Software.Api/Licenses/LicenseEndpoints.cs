using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Software.Api.Licenses;

public enum LicenseType { PerSeat, Site, OpenSource }

[JsonConverter(typeof(SoftwareVersionConverter))]
public record SoftwareVersion(int Major, int Minor, int Patch);

public record License
{
    [Required]
    public string Product { get; init; } = string.Empty;
    public LicenseType Type { get; init; }
    public SoftwareVersion? Version { get; init; }
    public DateOnly Expires { get; init; }
    [JsonPropertyName("seat_count"), Range(1, 100)]
    public int Seats { get; init; } = 1;
}

// Just enough to look at what goes on the wire: a hard-coded sample and no database.
public static class LicenseEndpoints
{
    private static readonly License Sample = new()
    {
        Product = "Rider",
        Type = LicenseType.PerSeat,
        Version = new SoftwareVersion(2026, 1, 3),
        Expires = new DateOnly(2027, 1, 1),
        Seats = 12
    };

    public static IEndpointRouteBuilder MapLicenseEndpoints(this IEndpointRouteBuilder app)
    {
        var licenses = app.MapGroup("/licenses");

        licenses.MapGet("", GetLicense);
        licenses.MapPost("", AddLicense);

        return app;
    }

    private static Ok<License> GetLicense()
    {
        return TypedResults.Ok(Sample);
    }

    // It doesn't save anything. It sends back what it understood.
    private static Ok<License> AddLicense(License request)
    {
        return TypedResults.Ok(request);
    }
}
