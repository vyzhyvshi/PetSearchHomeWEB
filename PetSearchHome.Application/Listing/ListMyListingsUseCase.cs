using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;

namespace PetSearchHome_WEB.Application.Listing
{
    public sealed record ListMyListingsRequest();

    public class ListMyListingsUseCase : IUseCase<ListMyListingsRequest, Result<IReadOnlyList<PetListing>>>
    {
        private readonly IListingRepository _listings;

        public ListMyListingsUseCase(IListingRepository listings)
        {
            _listings = listings;
        }

        public async Task<Result<IReadOnlyList<PetListing>>> ExecuteAsync(ListMyListingsRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null || !ListingAccessPolicy.CanCreate(authContext.Role))
            {
                return Result.Failure<IReadOnlyList<PetListing>>("Необхідна авторизація для перегляду ваших оголошень.");
            }

            var listings = await _listings.ListByOwnerAsync(authContext.UserId.Value, cancellationToken);
            return Result.Success(listings);
        }
    }
}