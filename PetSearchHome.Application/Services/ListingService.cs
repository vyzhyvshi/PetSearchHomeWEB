using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Services
{
    public class ListingService
    {
        private readonly IListingRepository _repository;

        public ListingService(IListingRepository repository)
        {
            _repository = repository;
        }

        public Task<IReadOnlyList<PetListing>> GetFeaturedAsync(int take = 6, CancellationToken cancellationToken = default)
        {
            return _repository.GetFeaturedAsync(take, cancellationToken);
        }

        public Task AddAsync(PetListing listing, CancellationToken cancellationToken = default)
        {
            return _repository.AddAsync(listing, cancellationToken);
        }
    }
}
