namespace PetSearchHome_WEB.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public int RecipientId { get; init; }
           
        public string Message { get; init; } = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
            = DateTimeOffset.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}
