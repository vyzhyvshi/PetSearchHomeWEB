namespace PetSearchHome_WEB.Domain.Entities
{
    public class Favorite
    {
        public Guid UserId { get; init; }
            = Guid.Empty;
        public Guid ListingId { get; init; }
            = Guid.Empty;
        public DateTimeOffset CreatedAt { get; init; }
            = DateTimeOffset.UtcNow;
    }
}
