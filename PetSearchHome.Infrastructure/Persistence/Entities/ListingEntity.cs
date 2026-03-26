using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Entities;

public class ListingEntity
{
    public int ListingId { get; set; }
    public Guid DomainId { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AnimalType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsUrgent { get; set; }
    public string Breed { get; set; } = string.Empty;
    public int AgeMonths { get; set; }
        = 0;
    public Sex Sex { get; set; }
        = Sex.Unknown;
    public PetSize Size { get; set; }
        = PetSize.Unknown;
    public string Color { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ListingStatus Status { get; set; }
        = ListingStatus.PendingModeration;
    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public UserEntity User { get; set; } = null!;
    public ICollection<PhotoEntity> Photos { get; set; }
        = new List<PhotoEntity>();
    public HealthInfoEntity? HealthInfo { get; set; }
        = null;
    public ICollection<FavoriteEntity> Favorites { get; set; }
        = new List<FavoriteEntity>();
    public ICollection<ReportEntity> Reports { get; set; }
        = new List<ReportEntity>();
}
