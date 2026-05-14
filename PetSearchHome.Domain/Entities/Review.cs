namespace PetSearchHome_WEB.Domain.Entities
{
    public class Review
    {
        public int Id { get; init; } 
        public int ReviewedUserId { get; init; }
            
        public int AuthorId { get; init; }
           
        public byte Rating { get; init; }
            = 0;
        public string Comment { get; init; }
            = string.Empty;
        public DateTimeOffset CreatedAt { get; init; }
            = DateTimeOffset.UtcNow;
    }
}
