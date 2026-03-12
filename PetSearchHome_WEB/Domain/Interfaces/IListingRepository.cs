using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IListingRepository
    {
        Task<IReadOnlyList<PetListing>> GetFeaturedAsync(int take, CancellationToken cancellationToken = default);
        Task AddAsync(PetListing listing, CancellationToken cancellationToken = default);
    }
}
