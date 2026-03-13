namespace PetSearchHome_WEB.Domain.Entities
{
    public class OrgStats
    {
        public Guid ShelterId { get; init; }
            = Guid.Empty;
        public int TotalListings { get; init; }
            = 0;
        public int AdoptedCount { get; init; }
            = 0;
        public int RejectedCount { get; init; }
            = 0;
    }
}
