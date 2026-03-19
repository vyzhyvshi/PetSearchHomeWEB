namespace PetSearchHome_WEB.Infrastructure.Persistence.Entities;

public class ShelterProfileEntity
{
    public int ShelterId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
        = null;
    public float Rating { get; set; }
        = 0f;

    public UserEntity User { get; set; } = null!;
}
