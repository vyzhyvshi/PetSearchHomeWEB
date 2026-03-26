using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Configurations;

public class ListingEntityConfiguration : IEntityTypeConfiguration<ListingEntity>
{
    public void Configure(EntityTypeBuilder<ListingEntity> builder)
    {
        builder.ToTable("listings");
        builder.HasKey(l => l.ListingId);
        builder.Property(l => l.ListingId)
            .HasColumnName("listing_id");

        builder.Property(l => l.DomainId)
            .HasColumnName("domain_id")
            .HasDefaultValueSql("gen_random_uuid()")
            .IsRequired();

        builder.Property(l => l.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(l => l.Title)
            .HasColumnName("title")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(l => l.AnimalType)
            .HasColumnName("animal_type")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(l => l.Location)
            .HasColumnName("location")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(l => l.IsUrgent)
            .HasColumnName("is_urgent")
            .HasDefaultValue(false)
            .IsRequired();

        builder.Property(l => l.Breed)
            .HasColumnName("breed")
            .HasMaxLength(128);
        builder.Property(l => l.AgeMonths)
            .HasColumnName("age_months");
        builder.Property(l => l.Sex)
            .HasColumnName("sex")
            .IsRequired();
        builder.Property(l => l.Size)
            .HasColumnName("size")
            .IsRequired();
        builder.Property(l => l.Color)
            .HasColumnName("color")
            .HasMaxLength(64);
        builder.Property(l => l.City)
            .HasColumnName("city")
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(l => l.District)
            .HasColumnName("district")
            .HasMaxLength(128);
        builder.Property(l => l.Description)
            .HasColumnName("description");
        builder.Property(l => l.Status)
            .HasColumnName("status")
            .IsRequired();
        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.HasIndex(l => l.DomainId)
            .IsUnique();
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.City);

        builder.HasOne(l => l.User)
            .WithMany(u => u.Listings)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.HealthInfo)
            .WithOne(h => h.Listing)
            .HasForeignKey<HealthInfoEntity>(h => h.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Photos)
            .WithOne(p => p.Listing)
            .HasForeignKey(p => p.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(l => l.Reports);
    }
}
