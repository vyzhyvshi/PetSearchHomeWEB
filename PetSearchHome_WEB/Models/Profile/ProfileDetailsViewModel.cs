using PetSearchHome_WEB.Domain.ValueObjects;
using PetSearchHome_WEB.Models.Listing;

namespace PetSearchHome_WEB.Models.Profile
{
    public class ProfileDetailsViewModel
    {
        public Guid UserId { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public Role Role { get; init; }
        public bool IsBlocked { get; init; }
        public string? Description { get; init; }
        public string? Website { get; init; }
        public string? Address { get; init; }
        public double? Rating { get; init; }
        public int ReviewsCount { get; init; }
        public IReadOnlyList<ListingWithStatusViewModel> Listings { get; init; } = Array.Empty<ListingWithStatusViewModel>();
        public bool CanEdit { get; init; }
        public bool CanReport { get; init; }
        public bool CanLeaveReview { get; init; }
        public bool CanChat { get; init; }
        public bool IsAuthenticatedViewer { get; init; }
        public bool IsOwnProfile { get; init; }
    }

    public class ListingWithStatusViewModel : ListingSummaryViewModel
    {
        public ListingStatus Status { get; init; }
    }
}
