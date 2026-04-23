using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Configurations;

public class ChatConversationEntityConfiguration : IEntityTypeConfiguration<ChatConversationEntity>
{
    public void Configure(EntityTypeBuilder<ChatConversationEntity> builder)
    {
        builder.ToTable("chat_conversations");
        builder.HasKey(c => c.ConversationId);

        builder.Property(c => c.ConversationId)
            .HasColumnName("conversation_id");
        builder.Property(c => c.UserAId)
            .HasColumnName("user_a_id")
            .IsRequired();
        builder.Property(c => c.UserBId)
            .HasColumnName("user_b_id")
            .IsRequired();
        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(c => new { c.UserAId, c.UserBId })
            .IsUnique();

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(c => c.UserAId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(c => c.UserBId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
