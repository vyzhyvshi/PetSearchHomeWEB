using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Listing
{
    public sealed record EditListingRequest(
        Guid ListingId,
        string Title,
        string AnimalType,
        string Location,
        string? Description,
        bool IsUrgent);

    public class EditListingUseCase : IUseCase<EditListingRequest, bool>
    {
        private readonly IListingRepository _listings;

        public EditListingUseCase(IListingRepository listings)
        {
            _listings = listings;
        }

        public async Task<bool> ExecuteAsync(EditListingRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            var listing = await _listings.GetByIdAsync(request.ListingId, cancellationToken)
                ?? throw new InvalidOperationException("Listing not found.");

            if (!ListingAccessPolicy.CanManage(authContext.Role, listing.OwnerId, authContext.UserId))
            {
                throw new UnauthorizedAccessException("Cannot edit listing.");
            }

            var updated = listing with
            {
                Title = request.Title,
                AnimalType = request.AnimalType,
                Location = request.Location,
                Description = request.Description,
                IsUrgent = request.IsUrgent,
                Status = ListingStatus.PendingModeration
            };

            await _listings.UpdateAsync(updated, cancellationToken);
            return true;
        }
    }
}
