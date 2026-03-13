using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IListingRepository
    {
        Task<IReadOnlyList<PetListing>> GetFeaturedAsync(int take, CancellationToken cancellationToken = default);
        Task<PetListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PetListing>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default);
        Task AddAsync(PetListing listing, CancellationToken cancellationToken = default);
        Task UpdateAsync(PetListing listing, CancellationToken cancellationToken = default);
        Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
