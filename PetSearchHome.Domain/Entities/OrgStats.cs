namespace PetSearchHome_WEB.Domain.Entities
{
    public class OrgStats
    {
        public int ShelterId { get; init; }
        public int TotalListings { get; init; }
            = 0;
        public int AdoptedCount { get; init; }
            = 0;
        public int RejectedCount { get; init; }
            = 0;
    }
}
