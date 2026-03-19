namespace PetSearchHome_WEB.Domain.Entities
{
    public class Review
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid ListingId { get; init; }
            = Guid.Empty;
        public Guid AuthorId { get; init; }
            = Guid.Empty;
        public byte Rating { get; init; }
            = 0;
        public string Comment { get; init; }
            = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
            = DateTimeOffset.UtcNow;
    }
}
