using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Persistence.Configurations;

public class ReportEntityConfiguration : IEntityTypeConfiguration<ReportEntity>
{
    public void Configure(EntityTypeBuilder<ReportEntity> builder)
    {
        builder.ToTable("reports");
        builder.HasKey(r => r.ReportId);

        builder.Property(r => r.ReportId)
            .HasColumnName("report_id");
        builder.Property(r => r.ReporterId)
            .HasColumnName("reporter_id")
            .IsRequired();
        builder.Property(r => r.ReportedType)
            .HasColumnName("reported_type")
            .IsRequired();
        builder.Property(r => r.ReportedId)
            .HasColumnName("reported_id")
            .IsRequired();
        builder.Property(r => r.Status)
            .HasColumnName("status")
            .IsRequired();
        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // map the text column
        builder.Property(r => r.Text)
            .HasColumnName("text")
            .HasColumnType("text")
            .IsRequired(false);

        builder.HasOne(r => r.Reporter)
            .WithMany(u => u.ReportsFiled)
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
