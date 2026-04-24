namespace PetSearchHome_WEB.Domain.Entities
{
    public class ChatMessage
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public Guid ConversationId { get; init; }
        public Guid SenderId { get; init; }
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; init; }
        public DateTimeOffset SentAt { get; init; } = DateTimeOffset.UtcNow;
    }
}
