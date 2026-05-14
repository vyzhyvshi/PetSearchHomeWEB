using Microsoft.EntityFrameworkCore;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Infrastructure.Persistence;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

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
            var uId = favorite.UserId;

            var listingEntity = await _db.Listings
                .FirstOrDefaultAsync(l => l.DomainId == favorite.ListingId, cancellationToken);

            if (listingEntity == null) return; 

            var entity = new FavoriteEntity
            {
                UserId = uId,
                ListingId = listingEntity.ListingId
            };

            await _db.Favorites.AddAsync(entity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveAsync(int userId, int listingId, CancellationToken cancellationToken = default)
        {
            var uId = userId;

            var listingEntity = await _db.Listings
                .FirstOrDefaultAsync(l => l.DomainId == listingId, cancellationToken);

            if (listingEntity == null) return;

            var entity = await _db.Favorites
                .FirstOrDefaultAsync(f => f.UserId == uId && f.ListingId == listingEntity.ListingId, cancellationToken);

            if (entity != null)
            {
                _db.Favorites.Remove(entity);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<Favorite?> GetAsync(int userId, int listingId, CancellationToken cancellationToken = default)
        {
            var uId = userId;

            var listingEntity = await _db.Listings
                .FirstOrDefaultAsync(l => l.DomainId == listingId, cancellationToken);

            if (listingEntity == null) return null;

            var entity = await _db.Favorites
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.UserId == uId && f.ListingId == listingEntity.ListingId, cancellationToken);

            if (entity == null) return null;

            return new Favorite
            {
                UserId = userId,
                ListingId = listingId
            };
        }

        public async Task<IReadOnlyList<int>> ListIdsByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var uId = userId;

            var query = from f in _db.Favorites
                        join l in _db.Listings on f.ListingId equals l.ListingId
                        where f.UserId == uId
                        select l.DomainId;

            return await query.ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Favorite>> ListByUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var uId = userId;

            var query = from f in _db.Favorites
                        join l in _db.Listings on f.ListingId equals l.ListingId
                        where f.UserId == uId
                        select new Favorite
                        {
                            UserId = userId,
                            ListingId = l.DomainId
                        };

            return await query.ToListAsync(cancellationToken);
        }

        
    }
}