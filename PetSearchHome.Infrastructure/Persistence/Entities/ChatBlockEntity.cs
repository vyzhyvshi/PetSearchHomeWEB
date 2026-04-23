namespace PetSearchHome_WEB.Infrastructure.Persistence.Entities;

public class ChatBlockEntity
{
    public int ChatBlockId { get; set; }
    public int BlockerId { get; set; }
    public int BlockedId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
