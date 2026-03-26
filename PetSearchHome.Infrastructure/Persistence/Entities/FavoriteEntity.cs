namespace PetSearchHome_WEB.Infrastructure.Persistence.Entities;

public class FavoriteEntity
{
    public int FavoriteId { get; set; }
    public int UserId { get; set; }
    public int ListingId { get; set; }
    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public UserEntity User { get; set; } = null!;
    public ListingEntity Listing { get; set; } = null!;
}
