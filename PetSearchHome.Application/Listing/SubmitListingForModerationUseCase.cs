using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Listing
{
    public sealed record SubmitListingForModerationRequest(Guid ListingId);

    public class SubmitListingForModerationUseCase : IUseCase<SubmitListingForModerationRequest, Result<bool>>
    {
        private readonly IListingRepository _listings;
        private readonly IModerationQueue _moderationQueue;

        public SubmitListingForModerationUseCase(IListingRepository listings, IModerationQueue moderationQueue)
        {
            _listings = listings;
            _moderationQueue = moderationQueue;
        }

        public async Task<Result<bool>> ExecuteAsync(SubmitListingForModerationRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var listing = await _listings.GetByIdAsync(request.ListingId, cancellationToken);
            if (listing == null)
            {
                return Result.Failure<bool>("Оголошення не знайдено.");
            }

            if (!ListingAccessPolicy.CanManage(authContext.Role, listing.OwnerId, authContext.UserId))
            {
                return Result.Failure<bool>("У вас немає прав для відправки на модерацію.");
            }

            var needsModeration = ModerationPolicy.RequiresModeration(authContext.Role);
            if (!needsModeration)
            {
                return Result.Success(true);
            }

            var updated = listing with { Status = ListingStatus.PendingModeration };
            await _listings.UpdateAsync(updated, cancellationToken);
            await _moderationQueue.EnqueueAsync(updated, cancellationToken);

            return Result.Success(true);
        }
    }
}