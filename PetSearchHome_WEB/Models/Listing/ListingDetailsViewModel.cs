using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Models.Listing
{
    public class ListingDetailsViewModel
    {
        public PetListing Listing { get; init; } = new();
        public bool IsFavorite { get; init; }
        public Guid OwnerId { get; init; }
        public string OwnerDisplayName { get; init; } = string.Empty;
    }
}
