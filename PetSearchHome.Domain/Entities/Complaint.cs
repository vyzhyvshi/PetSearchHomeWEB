namespace PetSearchHome_WEB.Domain.Entities
{
    public class Complaint
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid ListingId { get; init; }
            = Guid.Empty;
        public Guid ReporterId { get; init; }
            = Guid.Empty;
        public string Reason { get; init; }
            = string.Empty;
        public string? Status { get; init; }
            = "pending";
        public DateTimeOffset CreatedAt { get; init; }
            = DateTimeOffset.UtcNow;
    }
}
