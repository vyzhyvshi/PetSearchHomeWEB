using PetSearchHome_WEB.Domain.Entities;

namespace PetSearchHome_WEB.Domain.Interfaces
{
    public interface IReviewRepository
    {
        Task AddAsync(Review review, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Review>> ListByReviewedUserAsync(int reviewedUserId, CancellationToken cancellationToken = default);
    }
}
