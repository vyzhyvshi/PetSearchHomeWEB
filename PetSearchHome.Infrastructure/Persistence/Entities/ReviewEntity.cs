namespace PetSearchHome_WEB.Infrastructure.Persistence.Entities;

public class ReviewEntity
{
    public int ReviewId { get; set; }
    public int ReviewerId { get; set; }
    public int ReviewedId { get; set; }
    public int Rating { get; set; }
        = 0;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public UserEntity Reviewer { get; set; } = null!;
    public UserEntity ReviewedUser { get; set; } = null!;
}
