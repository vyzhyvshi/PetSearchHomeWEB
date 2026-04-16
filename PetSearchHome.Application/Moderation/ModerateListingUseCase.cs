using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Moderation
{
    public sealed record ModerateListingRequest(Guid ListingId, bool Approve, string? Reason);

    public class ModerateListingUseCase : IUseCase<ModerateListingRequest, Result<bool>>
    {
        private readonly IListingRepository _listings;
        private readonly INotificationGateway _notifications;
        private readonly IAuditLogGateway _audit;

        public ModerateListingUseCase(IListingRepository listings, INotificationGateway notifications, IAuditLogGateway audit)
        {
            _listings = listings;
            _notifications = notifications;
            _audit = audit;
        }

        public async Task<Result<bool>> ExecuteAsync(ModerateListingRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (!AdminPolicy.IsAdmin(authContext.Role))
            {
                return Result.Failure<bool>("Немає прав доступу. Потрібна роль Адміністратора.");
            }

            var listing = await _listings.GetByIdAsync(request.ListingId, cancellationToken);
            if (listing == null)
            {
                return Result.Failure<bool>("Оголошення не знайдено.");
            }

            var newStatus = request.Approve ? ListingStatus.Published : ListingStatus.Rejected;
            var updated = listing with { Status = newStatus };

            await _listings.UpdateAsync(updated, cancellationToken);
            await _notifications.NotifyAsync(listing.OwnerId, $"Ваше оголошення '{listing.Title}' було {(request.Approve ? "схвалено" : "відхилено")}.", cancellationToken);
            await _audit.RecordAsync("moderate_listing", authContext.UserId ?? Guid.Empty, listing.Id.ToString(), cancellationToken);

            return Result.Success(true);
        }
    }
}