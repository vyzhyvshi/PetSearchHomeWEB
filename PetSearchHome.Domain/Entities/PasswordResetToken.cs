namespace PetSearchHome_WEB.Domain.Entities
{
    public class PasswordResetToken
    {
        public int Id { get; init; } 
        public int UserId { get; init; }
        public string TokenHash { get; init; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; init; }
        public DateTimeOffset? UsedAt { get; init; }
        public bool IsUsable => UsedAt is null && ExpiresAt > DateTimeOffset.UtcNow;
    }
}
