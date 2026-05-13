namespace PetSearchHome_WEB.Domain.Entities
{
    public class ShelterProfile
    {
        public int ShelterId { get; init; }
            
        public string DisplayName { get; init; }
            = string.Empty;
        public string Description { get; init; }
            = string.Empty;
        public string? Website { get; init; }
            = null;
        public string? Address { get; init; }
            = null;
    }
}
