namespace PetSearchHome_WEB.Domain.Entities
{
    public class PetListing
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Title { get; init; } = string.Empty;
        public string AnimalType { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public DateTimeOffset ListedAt { get; init; }
            = DateTimeOffset.UtcNow;
        public string? Description { get; init; }
            = null;
        public bool IsUrgent { get; init; }
            = false;
    }
}
