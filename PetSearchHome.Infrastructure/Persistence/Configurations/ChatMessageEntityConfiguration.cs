using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Configurations;

public class ChatMessageEntityConfiguration : IEntityTypeConfiguration<ChatMessageEntity>
{
    public void Configure(EntityTypeBuilder<ChatMessageEntity> builder)
    {
        builder.ToTable("chat_messages");
        builder.HasKey(m => m.MessageId);

        builder.Property(m => m.MessageId)
            .HasColumnName("message_id");
        builder.Property(m => m.ConversationId)
            .HasColumnName("conversation_id")
            .IsRequired();
        builder.Property(m => m.SenderId)
            .HasColumnName("sender_id")
            .IsRequired();
        builder.Property(m => m.Content)
            .HasColumnName("content")
            .HasMaxLength(2000)
            .IsRequired();
        builder.Property(m => m.ImageUrl)
            .HasColumnName("image_url")
            .HasMaxLength(2048);
        builder.Property(m => m.SentAt)
            .HasColumnName("sent_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
