using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Entities;

public class UserEntity
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Role Role { get; set; } = Role.Person;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; } = false;

    public IndividualProfileEntity? IndividualProfile { get; set; }
        = null;
    public ShelterProfileEntity? ShelterProfile { get; set; }
        = null;
    public ICollection<ListingEntity> Listings { get; set; }
        = new List<ListingEntity>();
    public ICollection<FavoriteEntity> Favorites { get; set; }
        = new List<FavoriteEntity>();
    public ICollection<ReviewEntity> ReviewsWritten { get; set; }
        = new List<ReviewEntity>();
    public ICollection<ReviewEntity> ReviewsReceived { get; set; }
        = new List<ReviewEntity>();
    public ICollection<ReportEntity> ReportsFiled { get; set; }
        = new List<ReportEntity>();
}
