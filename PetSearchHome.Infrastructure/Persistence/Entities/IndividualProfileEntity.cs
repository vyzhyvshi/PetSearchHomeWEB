namespace PetSearchHome_WEB.Infrastructure.Persistence.Entities;

public class IndividualProfileEntity
{
    public int IndividualId { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string? AdditionalInfo { get; set; }
        = null;

    public UserEntity User { get; set; } = null!;
}
