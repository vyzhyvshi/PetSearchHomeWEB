namespace PetSearchHome_WEB.Infrastructure.Persistence.Entities;

public class ChatMessageEntity
{
    public int MessageId { get; set; }
    public int ConversationId { get; set; }
    public int SenderId { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public ChatConversationEntity Conversation { get; set; } = null!;
}
