using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Entities;

public class ReportEntity
{
    public int ReportId { get; set; }
    public int ReporterId { get; set; }
    public ReportedEntityType ReportedType { get; set; }
        = ReportedEntityType.Listing;
    public int ReportedId { get; set; }
    public ReportStatus Status { get; set; }
        = ReportStatus.Pending;
    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;
    public string? Text { get; set; } // Nullable text body of the report

    public UserEntity Reporter { get; set; } = null!;
}
