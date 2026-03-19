using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Configurations;

public class PhotoEntityConfiguration : IEntityTypeConfiguration<PhotoEntity>
{
    public void Configure(EntityTypeBuilder<PhotoEntity> builder)
    {
        builder.ToTable("photos");
        builder.HasKey(p => p.PhotoId);

        builder.Property(p => p.PhotoId)
            .HasColumnName("photo_id");
        builder.Property(p => p.ListingId)
            .HasColumnName("listing_id")
            .IsRequired();
        builder.Property(p => p.Url)
            .HasColumnName("url")
            .HasMaxLength(512)
            .IsRequired();
        builder.Property(p => p.IsPrimary)
            .HasColumnName("is_primary")
            .HasDefaultValue(false);

        builder.HasOne(p => p.Listing)
            .WithMany(l => l.Photos)
            .HasForeignKey(p => p.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
