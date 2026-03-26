using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Configurations;

public class ShelterProfileEntityConfiguration : IEntityTypeConfiguration<ShelterProfileEntity>
{
    public void Configure(EntityTypeBuilder<ShelterProfileEntity> builder)
    {
        builder.ToTable("shelter_profiles");
        builder.HasKey(s => s.ShelterId);
        builder.Property(s => s.ShelterId)
            .HasColumnName("shelter_id");
        builder.Property(s => s.UserId)
            .HasColumnName("user_id")
            .IsRequired();
        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(256)
            .IsRequired();
        builder.Property(s => s.ContactPerson)
            .HasColumnName("contact_person")
            .HasMaxLength(128)
            .IsRequired();
        builder.Property(s => s.Phone)
            .HasColumnName("phone")
            .HasMaxLength(32);
        builder.Property(s => s.Address)
            .HasColumnName("address")
            .HasMaxLength(256);
        builder.Property(s => s.Description)
            .HasColumnName("description");
        builder.Property(s => s.Rating)
            .HasColumnName("rating")
            .HasDefaultValue(0f);

        builder.HasOne(s => s.User)
            .WithOne(u => u.ShelterProfile)
            .HasForeignKey<ShelterProfileEntity>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
