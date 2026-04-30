using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Configurations;

public class PasswordResetTokenEntityConfiguration : IEntityTypeConfiguration<PasswordResetTokenEntity>
{
    public void Configure(EntityTypeBuilder<PasswordResetTokenEntity> builder)
    {
        builder.ToTable("password_reset_tokens");
        builder.HasKey(t => t.PasswordResetTokenId);

        builder.Property(t => t.PasswordResetTokenId)
            .HasColumnName("password_reset_token_id");
        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        builder.Property(t => t.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(t => t.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();
        builder.Property(t => t.UsedAt)
            .HasColumnName("used_at");
        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(t => new { t.UserId, t.TokenHash });

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
