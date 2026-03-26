using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Configurations;

public class HealthInfoEntityConfiguration : IEntityTypeConfiguration<HealthInfoEntity>
{
    public void Configure(EntityTypeBuilder<HealthInfoEntity> builder)
    {
        builder.ToTable("health_infos");
        builder.HasKey(h => h.HealthId);

        builder.Property(h => h.HealthId)
            .HasColumnName("health_id");
        builder.Property(h => h.ListingId)
            .HasColumnName("listing_id")
            .IsRequired();
        builder.Property(h => h.Vaccinations)
            .HasColumnName("vaccinations");
        builder.Property(h => h.Sterilized)
            .HasColumnName("sterilized");
        builder.Property(h => h.ChronicDiseases)
            .HasColumnName("chronic_diseases");

        builder.HasOne(h => h.Listing)
            .WithOne(l => l.HealthInfo)
            .HasForeignKey<HealthInfoEntity>(h => h.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
