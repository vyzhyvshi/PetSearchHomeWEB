using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;

namespace PetSearchHome_WEB.Application.Listing
{
    public sealed record DeleteListingRequest(Guid ListingId);

    public class DeleteListingUseCase : IUseCase<DeleteListingRequest, Result<bool>>
    {
        private readonly IListingRepository _listings;

        public DeleteListingUseCase(IListingRepository listings)
        {
            _listings = listings;
        }

        public async Task<Result<bool>> ExecuteAsync(DeleteListingRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var listing = await _listings.GetByIdAsync(request.ListingId, cancellationToken);
            if (listing == null)
            {
                return Result.Failure<bool>("Оголошення не знайдено.");
            }

            if (!ListingAccessPolicy.CanManage(authContext.Role, listing.OwnerId, authContext.UserId))
            {
                return Result.Failure<bool>("У вас немає прав для видалення цього оголошення.");
            }

            await _listings.RemoveAsync(request.ListingId, cancellationToken);
            return true;
        }
    }
}
