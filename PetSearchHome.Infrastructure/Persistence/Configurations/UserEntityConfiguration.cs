using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Configurations;

public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserId)
            .HasColumnName("user_id");
        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();
        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(512)
            .IsRequired();
        builder.Property(u => u.Role)
            .HasColumnName("role")
            .IsRequired();
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);
        builder.Property(u => u.IsVerified)
            .HasColumnName("is_verified")
            .HasDefaultValue(false);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasOne(u => u.IndividualProfile)
            .WithOne(p => p.User)
            .HasForeignKey<IndividualProfileEntity>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.ShelterProfile)
            .WithOne(p => p.User)
            .HasForeignKey<ShelterProfileEntity>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
