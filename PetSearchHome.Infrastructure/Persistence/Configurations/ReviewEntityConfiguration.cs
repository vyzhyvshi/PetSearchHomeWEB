using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Configurations;

public class ReviewEntityConfiguration : IEntityTypeConfiguration<ReviewEntity>
{
    public void Configure(EntityTypeBuilder<ReviewEntity> builder)
    {
        builder.ToTable("reviews");
        builder.HasKey(r => r.ReviewId);

        builder.Property(r => r.ReviewId)
            .HasColumnName("review_id");
        builder.Property(r => r.ReviewerId)
            .HasColumnName("reviewer_id")
            .IsRequired();
        builder.Property(r => r.ReviewedId)
            .HasColumnName("reviewed_id")
            .IsRequired();
        builder.Property(r => r.Rating)
            .HasColumnName("rating")
            .IsRequired();
        builder.Property(r => r.Comment)
            .HasColumnName("comment");
        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasOne(r => r.Reviewer)
            .WithMany(u => u.ReviewsWritten)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.ReviewedUser)
            .WithMany(u => u.ReviewsReceived)
            .HasForeignKey(r => r.ReviewedId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
