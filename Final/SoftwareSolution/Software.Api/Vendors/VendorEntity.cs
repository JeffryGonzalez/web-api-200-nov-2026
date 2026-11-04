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

    // Changes on every save. A save only works if this still matches what's in the database.
    [Version] public Guid Version { get; set; }
}