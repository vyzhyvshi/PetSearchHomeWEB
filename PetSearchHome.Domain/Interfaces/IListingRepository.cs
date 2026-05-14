using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IListingRepository
    {
        Task<IReadOnlyList<PetListing>> GetFeaturedAsync(int take, CancellationToken cancellationToken = default);
        Task<PetListing?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PetListing>> ListByOwnerAsync(int ownerId, CancellationToken cancellationToken = default);
        Task AddAsync(PetListing listing, CancellationToken cancellationToken = default);
        Task UpdateAsync(PetListing listing, CancellationToken cancellationToken = default);
        Task RemoveAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PetListing>> ListByStatusAsync(ValueObjects.ListingStatus status, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<PetListing>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    }
}
