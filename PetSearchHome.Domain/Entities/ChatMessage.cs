namespace PetSearchHome_WEB.Domain.Entities
{
    public class ChatMessage
    {
        public int Id { get; init; } 
        public int ConversationId { get; init; }
        public int SenderId { get; init; }
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; init; }
        public DateTimeOffset SentAt { get; init; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? ReadAt { get; init; }
    }
}
