using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;

namespace PetSearchHome_WEB.Application.Listing
{
    public sealed record DeleteListingRequest(Guid ListingId);

    public class DeleteListingUseCase : IUseCase<DeleteListingRequest, bool>
    {
        private readonly IListingRepository _listings;

        public DeleteListingUseCase(IListingRepository listings)
        {
            _listings = listings;
        }

        public async Task<bool> ExecuteAsync(DeleteListingRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var listing = await _listings.GetByIdAsync(request.ListingId, cancellationToken)
                ?? throw new InvalidOperationException("Listing not found.");

            if (!ListingAccessPolicy.CanManage(authContext.Role, listing.OwnerId, authContext.UserId))
            {
                throw new UnauthorizedAccessException("Cannot delete listing.");
            }

            await _listings.RemoveAsync(request.ListingId, cancellationToken);
            return true;
        }
    }
}
