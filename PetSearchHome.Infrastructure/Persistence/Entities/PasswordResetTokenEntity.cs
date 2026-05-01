namespace PetSearchHome_WEB.Infrastructure.Persistence.Entities;

public class PasswordResetTokenEntity
{
    public int PasswordResetTokenId { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UserEntity User { get; set; } = null!;
}
