namespace PetSearchHome_WEB.Domain.Entities
{
    public class Favorite
    {
        public int UserId { get; init; }
           
        public int ListingId { get; init; }
            
        public DateTimeOffset CreatedAt { get; init; }
            = DateTimeOffset.UtcNow;
    }
}
