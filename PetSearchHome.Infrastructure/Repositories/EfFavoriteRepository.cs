using Microsoft.EntityFrameworkCore;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Infrastructure.Persistence;

namespace PetSearchHome_WEB.Infrastructure.Repositories
{
    public class EfFavoriteRepository : IFavoriteRepository
    {
        private readonly ApplicationDbContext _db;

        public EfFavoriteRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default)
        {
            await _db.Set<Favorite>().AddAsync(favorite, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default)
        {
            var favorite = await _db.Set<Favorite>()
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ListingId == listingId, cancellationToken);

            if (favorite != null)
            {
                _db.Set<Favorite>().Remove(favorite);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<Favorite?> GetAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default)
        {
            return await _db.Set<Favorite>()
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ListingId == listingId, cancellationToken);
        }

        public async Task<IReadOnlyList<Guid>> ListIdsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.Set<Favorite>()
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Select(f => f.ListingId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Favorite>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _db.Set<Favorite>()
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .ToListAsync(cancellationToken);
        }
    }
}