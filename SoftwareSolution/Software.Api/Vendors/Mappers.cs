using Marten.Linq.SoftDeletes;

namespace Software.Api.Vendors;

public static class Mappers
{
    extension(VendorEntity entity)
    {
        public VendorDetailsModel MapToVendorsDetails()
        {
            return new VendorDetailsModel
            {
                Id = entity.Id,
                Name = entity.Name,
                PointOfContact = entity.PointOfContact,
                Url = entity.Url,
            };
        }

        
    }

    extension(VendorCreateModel request)
    {
        public VendorEntity MapToEntity(DateTimeOffset created, string by)
        {

            return new VendorEntity
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                PointOfContact = request.PointOfContact,
                Url = request.Url,
                CreatedAt = created,
                CreatedBy = by
            };
        }
    }
}
