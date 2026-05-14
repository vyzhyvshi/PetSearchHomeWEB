using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IFavoriteRepository
    {
        Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default);
        Task RemoveAsync(int userId, int listingId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Favorite>> ListByUserAsync(int userId, CancellationToken cancellationToken = default);
        Task<Favorite?> GetAsync(int userId, int listingId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<int>> ListIdsByUserAsync(int userId, CancellationToken cancellationToken = default);
    }
}
