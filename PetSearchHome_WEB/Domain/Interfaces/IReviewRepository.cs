using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IReviewRepository
    {
        Task AddAsync(Review review, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Review>> ListByListingAsync(Guid listingId, CancellationToken cancellationToken = default);
    }
}
