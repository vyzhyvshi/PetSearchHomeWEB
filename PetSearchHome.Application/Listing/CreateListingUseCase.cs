using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Listing
{
    public sealed record CreateListingRequest(
        string Title,
        string AnimalType,
        string Location,
        string? Description,
        bool IsUrgent,
        IReadOnlyList<string> PhotoUrls);

    public class CreateListingUseCase : IUseCase<CreateListingRequest, Result<Guid>>
    {
        private readonly IListingRepository _listings;
        private readonly IModerationQueue _moderationQueue;

        public CreateListingUseCase(IListingRepository listings, IModerationQueue moderationQueue)
        {
            _listings = listings;
            _moderationQueue = moderationQueue;
        }

        public async Task<Result<Guid>> ExecuteAsync(CreateListingRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null)
            {
                return Result.Failure<Guid>("Необхідна авторизація.");
            }

            if (!ListingAccessPolicy.CanCreate(authContext.Role))
            {
                return Result.Failure<Guid>("Ваша роль не дозволяє створювати оголошення.");
            }

            var status = ModerationPolicy.RequiresModeration(authContext.Role)
                ? ListingStatus.PendingModeration
                : ListingStatus.Published;

            PetListing listing = new()
            {
                OwnerId = authContext.UserId.Value,
                OwnerRole = authContext.Role,
                Title = request.Title,
                AnimalType = request.AnimalType,
                Location = request.Location,
                Description = request.Description,
                IsUrgent = request.IsUrgent,
                PhotoUrls = request.PhotoUrls,
                Status = status,
                ListedAt = DateTimeOffset.UtcNow
            };

            await _listings.AddAsync(listing, cancellationToken);

            if (status == ListingStatus.PendingModeration)
            {
                await _moderationQueue.EnqueueAsync(listing, cancellationToken);
            }

            return listing.Id;
        }
    }
}
