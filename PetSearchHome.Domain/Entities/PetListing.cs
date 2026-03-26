using System.Collections.Generic;

namespace PetSearchHome_WEB.Domain.Entities
{
    public record PetListing
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid OwnerId { get; init; } = Guid.Empty;
        public ValueObjects.Role OwnerRole { get; init; } = ValueObjects.Role.Guest;
        public string Title { get; init; } = string.Empty;
        public string AnimalType { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public ValueObjects.ListingStatus Status { get; init; }
            = ValueObjects.ListingStatus.Published;
        public DateTimeOffset ListedAt { get; init; }
            = DateTimeOffset.UtcNow;
        public string? Description { get; init; }
            = null;
        public bool IsUrgent { get; init; }
            = false;
        public IReadOnlyList<string> PhotoUrls { get; init; }
            = System.Array.Empty<string>();
        public string? PrimaryPhotoUrl => PhotoUrls.Count > 0 ? PhotoUrls[0] : null;
    }
}
