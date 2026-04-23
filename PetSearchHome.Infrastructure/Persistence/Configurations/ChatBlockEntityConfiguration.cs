using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Configurations;

public class ChatBlockEntityConfiguration : IEntityTypeConfiguration<ChatBlockEntity>
{
    public void Configure(EntityTypeBuilder<ChatBlockEntity> builder)
    {
        builder.ToTable("chat_blocks");
        builder.HasKey(b => b.ChatBlockId);

        builder.Property(b => b.ChatBlockId)
            .HasColumnName("chat_block_id");
        builder.Property(b => b.BlockerId)
            .HasColumnName("blocker_id")
            .IsRequired();
        builder.Property(b => b.BlockedId)
            .HasColumnName("blocked_id")
            .IsRequired();
        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(b => new { b.BlockerId, b.BlockedId })
            .IsUnique();

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(b => b.BlockerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(b => b.BlockedId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
