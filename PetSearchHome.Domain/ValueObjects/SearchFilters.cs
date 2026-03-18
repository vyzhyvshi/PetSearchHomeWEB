namespace PetSearchHome_WEB.Domain.ValueObjects
{
    public class SearchFilters
    {
        public string? AnimalType { get; init; }
        public string? Location { get; init; }
        public bool? IsUrgent { get; init; }
    }
}
