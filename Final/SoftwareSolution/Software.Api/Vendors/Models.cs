using System.ComponentModel.DataAnnotations;

namespace Software.Api.Vendors;

public record VendorDetailsModel
{
    public Guid Id { get; set; }
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required VendorPointOfContact PointOfContact { get; init; }
}

[AtLeastOneOf(nameof(Email), nameof(Phone))]
public record VendorPointOfContact
{
    [Required]
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
}

public record VendorCreateModel
{
    // [Required], not the required keyword. That fails reading the JSON, before validation can name the field.
    [Required, MinLength(5), MaxLength(100)]
    public string Name { get; init; } = string.Empty;
    [Required, Url]
    public string Url { get; init; } = string.Empty;
    [Required]
    public VendorPointOfContact PointOfContact { get; init; } = null!;
}

public record VendorSummaryModel
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Url { get; init; }
}

public record VendorSummary
{
    public required IReadOnlyList<VendorSummaryModel> Vendors { get; init; }
    public required int Page  { get; init; }
    public required int PageSize { get; init; } 
}