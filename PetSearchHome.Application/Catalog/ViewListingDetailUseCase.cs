using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Catalog
{
    public sealed record ViewListingDetailRequest(Guid ListingId);

    public class ViewListingDetailUseCase : IUseCase<ViewListingDetailRequest, PetListing?>
    {
        private readonly IListingRepository _listings;

        public ViewListingDetailUseCase(IListingRepository listings)
        {
            _listings = listings;
        }

        public async Task<PetListing?> ExecuteAsync(ViewListingDetailRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var listing = await _listings.GetByIdAsync(request.ListingId, cancellationToken);
            if (listing is null)
            {
                return null;
            }

            if (listing.Status == ListingStatus.Rejected && authContext.Role != Role.Admin && listing.OwnerId != authContext.UserId)
            {
                return null;
            }

            return listing;
        }
    }
}
