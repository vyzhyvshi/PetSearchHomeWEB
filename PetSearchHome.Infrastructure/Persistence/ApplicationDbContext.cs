using Microsoft.EntityFrameworkCore;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<ChatConversationEntity> ChatConversations => Set<ChatConversationEntity>();
    public DbSet<ChatMessageEntity> ChatMessages => Set<ChatMessageEntity>();
    public DbSet<ChatBlockEntity> ChatBlocks => Set<ChatBlockEntity>();
    public DbSet<IndividualProfileEntity> IndividualProfiles => Set<IndividualProfileEntity>();
    public DbSet<ShelterProfileEntity> ShelterProfiles => Set<ShelterProfileEntity>();
    public DbSet<ListingEntity> Listings => Set<ListingEntity>();
    public DbSet<PhotoEntity> Photos => Set<PhotoEntity>();
    public DbSet<HealthInfoEntity> HealthInfos => Set<HealthInfoEntity>();
    public DbSet<FavoriteEntity> Favorites => Set<FavoriteEntity>();
    public DbSet<ReviewEntity> Reviews => Set<ReviewEntity>();
    public DbSet<ReportEntity> Reports => Set<ReportEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}
