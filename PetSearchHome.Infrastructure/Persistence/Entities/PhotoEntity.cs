namespace PetSearchHome_WEB.Infrastructure.Persistence.Entities;

public class PhotoEntity
{
    public int PhotoId { get; set; }
    public int ListingId { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
        = false;

    public ListingEntity Listing { get; set; } = null!;
}
