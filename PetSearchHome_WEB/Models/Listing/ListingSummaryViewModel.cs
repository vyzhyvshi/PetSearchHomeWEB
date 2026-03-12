namespace PetSearchHome_WEB.Models.Listing
{
    public class ListingSummaryViewModel
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string AnimalType { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public DateTimeOffset ListedAt { get; init; }
            = DateTimeOffset.UtcNow;
        public bool IsUrgent { get; init; }
            = false;
    }
}
