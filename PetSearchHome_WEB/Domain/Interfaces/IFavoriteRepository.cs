using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IFavoriteRepository
    {
        Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default);
        Task RemoveAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Favorite>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
