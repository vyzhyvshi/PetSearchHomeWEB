using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Moderation
{
    public sealed record GetPendingListingsRequest();

    public class GetPendingListingsUseCase : IUseCase<GetPendingListingsRequest, Result<IReadOnlyList<PetListing>>>
    {
        private readonly IListingRepository _listings;

        public GetPendingListingsUseCase(IListingRepository listings)
        {
            _listings = listings;
        }

        public async Task<Result<IReadOnlyList<PetListing>>> ExecuteAsync(GetPendingListingsRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (!AdminPolicy.IsAdmin(authContext.Role))
            {
                return Result.Failure<IReadOnlyList<PetListing>>("Немає прав доступу. Потрібна роль Адміністратора.");
            }

            var listings = await _listings.ListByStatusAsync(ListingStatus.PendingModeration, cancellationToken);
            return Result.Success(listings);
        }
    }
}