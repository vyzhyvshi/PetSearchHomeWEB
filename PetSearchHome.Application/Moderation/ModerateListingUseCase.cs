using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Moderation
{
    public sealed record ModerateListingRequest(Guid ListingId, bool Approve, string? Reason);

    public class ModerateListingUseCase : IUseCase<ModerateListingRequest, bool>
    {
        private readonly IListingRepository _listings;
        private readonly INotificationGateway _notifications;
        private readonly IAuditLogGateway _audit;

        public ModerateListingUseCase(
            IListingRepository listings,
            INotificationGateway notifications,
            IAuditLogGateway audit)
        {
            _listings = listings;
            _notifications = notifications;
            _audit = audit;
        }

        public async Task<bool> ExecuteAsync(ModerateListingRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (!AdminPolicy.IsAdmin(authContext.Role))
            {
                throw new UnauthorizedAccessException("Admin role required.");
            }

            var listing = await _listings.GetByIdAsync(request.ListingId, cancellationToken)
                ?? throw new InvalidOperationException("Listing not found.");

            var newStatus = request.Approve ? ListingStatus.Published : ListingStatus.Rejected;
            var updated = listing with { Status = newStatus };

            await _listings.UpdateAsync(updated, cancellationToken);
            await _notifications.NotifyAsync(listing.OwnerId, $"Your listing '{listing.Title}' was {(request.Approve ? "approved" : "rejected")}.", cancellationToken);
            await _audit.RecordAsync("moderate_listing", authContext.UserId ?? Guid.Empty, listing.Id.ToString(), cancellationToken);
            return true;
        }
    }
}
