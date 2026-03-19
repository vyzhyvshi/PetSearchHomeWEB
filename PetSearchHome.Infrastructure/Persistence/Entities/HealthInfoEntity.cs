namespace PetSearchHome_WEB.Infrastructure.Persistence.Entities;

public class HealthInfoEntity
{
    public int HealthId { get; set; }
    public int ListingId { get; set; }
    public string? Vaccinations { get; set; }
        = null;
    public bool Sterilized { get; set; }
        = false;
    public string? ChronicDiseases { get; set; }
        = null;

    public ListingEntity Listing { get; set; } = null!;
}
