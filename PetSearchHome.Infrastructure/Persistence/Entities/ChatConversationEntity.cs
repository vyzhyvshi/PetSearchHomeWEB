namespace PetSearchHome_WEB.Infrastructure.Persistence.Entities;

public class ChatConversationEntity
{
    public int ConversationId { get; set; }
    public int UserAId { get; set; }
    public int UserBId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChatMessageEntity> Messages { get; set; } = new List<ChatMessageEntity>();
}
