using Marten.Schema;

namespace Software.Api.Vendors;

public class VendorEntity
{
    public Guid Id { get; set; }
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required VendorPointOfContact PointOfContact { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;

    // every time a vendor is changed, created, etc. it will generate a new version.
    [Version]
    public Guid Version { get; set; }
}