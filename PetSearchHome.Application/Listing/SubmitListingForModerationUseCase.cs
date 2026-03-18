using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Listing
{
    public sealed record SubmitListingForModerationRequest(Guid ListingId);

    public class SubmitListingForModerationUseCase : IUseCase<SubmitListingForModerationRequest, bool>
    {
        private readonly IListingRepository _listings;
        private readonly IModerationQueue _moderationQueue;

        public SubmitListingForModerationUseCase(IListingRepository listings, IModerationQueue moderationQueue)
        {
            _listings = listings;
            _moderationQueue = moderationQueue;
        }

        public async Task<bool> ExecuteAsync(SubmitListingForModerationRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var listing = await _listings.GetByIdAsync(request.ListingId, cancellationToken)
                ?? throw new InvalidOperationException("Listing not found.");

            if (!ListingAccessPolicy.CanManage(authContext.Role, listing.OwnerId, authContext.UserId))
            {
                throw new UnauthorizedAccessException("Cannot submit listing.");
            }

            var needsModeration = ModerationPolicy.RequiresModeration(authContext.Role);
            if (!needsModeration)
            {
                return true;
            }

            var updated = listing with { Status = ListingStatus.PendingModeration };
            await _listings.UpdateAsync(updated, cancellationToken);
            await _moderationQueue.EnqueueAsync(updated, cancellationToken);
            return true;
        }
    }
}
