namespace PetSearchHome_WEB.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid RecipientId { get; init; }
            = Guid.Empty;
        public string Message { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
            = DateTimeOffset.UtcNow;
        public bool IsRead { get; init; }
            = false;
    }
}
