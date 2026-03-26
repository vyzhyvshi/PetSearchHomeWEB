using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Configurations;

public class IndividualProfileEntityConfiguration : IEntityTypeConfiguration<IndividualProfileEntity>
{
    public void Configure(EntityTypeBuilder<IndividualProfileEntity> builder)
    {
        builder.ToTable("individual_profiles");
        builder.HasKey(p => p.IndividualId);
        builder.Property(p => p.IndividualId)
            .HasColumnName("individual_id");
        builder.Property(p => p.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        builder.Property(p => p.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(p => p.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(p => p.Phone)
            .HasColumnName("phone")
            .HasMaxLength(32);
        builder.Property(p => p.City)
            .HasColumnName("city")
            .HasMaxLength(128);
        builder.Property(p => p.District)
            .HasColumnName("district")
            .HasMaxLength(128);
        builder.Property(p => p.AdditionalInfo)
            .HasColumnName("additional_info");

        builder.HasOne(p => p.User)
            .WithOne(u => u.IndividualProfile)
            .HasForeignKey<IndividualProfileEntity>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
