using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Profiles
{
    public sealed record ViewProfileDetailsRequest(Guid UserId);

    public sealed record ProfileDetailsResult(
        User User,
        ShelterProfile? ShelterProfile,
        IReadOnlyList<PetListing> Listings,
        double? Rating,
        int ReviewsCount);

    public class ViewProfileDetailsUseCase : IUseCase<ViewProfileDetailsRequest, ProfileDetailsResult?>
    {
        private readonly IUserRepository _users;
        private readonly IListingRepository _listings;
        private readonly IShelterRepository _shelters;
        private readonly IReviewRepository _reviews;

        public ViewProfileDetailsUseCase(
            IUserRepository users,
            IListingRepository listings,
            IShelterRepository shelters,
            IReviewRepository reviews)
        {
            _users = users;
            _listings = listings;
            _shelters = shelters;
            _reviews = reviews;
        }

        public async Task<ProfileDetailsResult?> ExecuteAsync(
            ViewProfileDetailsRequest request,
            AuthContext authContext,
            CancellationToken cancellationToken = default)
        {
            var user = await _users.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return null;
            }

            var listings = await _listings.ListByOwnerAsync(request.UserId, cancellationToken);
            var visibleListings = authContext.Role == Role.Admin || authContext.UserId == request.UserId
                ? listings
                : listings.Where(static listing => listing.Status == ListingStatus.Published).ToList();

            var reviews = new List<Review>();
            foreach (var listing in listings)
            {
                reviews.AddRange(await _reviews.ListByListingAsync(listing.Id, cancellationToken));
            }

            double? rating = reviews.Count == 0
                ? null
                : Math.Round(reviews.Average(static review => review.Rating), 1, MidpointRounding.AwayFromZero);

            var shelterProfile = user.Role == Role.Shelter
                ? await _shelters.GetProfileAsync(request.UserId, cancellationToken)
                : null;

            return new ProfileDetailsResult(user, shelterProfile, visibleListings, rating, reviews.Count);
        }
    }
}
