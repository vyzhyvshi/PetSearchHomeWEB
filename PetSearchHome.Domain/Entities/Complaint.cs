using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Domain.Entities
{
    public class Complaint
    {
        public int Id { get; init; } 
        public ReportedEntityType ReportedType { get; init; }
            = ReportedEntityType.Listing;
        public int ReportedEntityId { get; init; }
         
        public int ReporterId { get; init; }
            
        public string Reason { get; init; }
            = string.Empty;
        public string? Status { get; init; }
            = "pending";
        public DateTimeOffset CreatedAt { get; init; }
            = DateTimeOffset.UtcNow;
    }
}
