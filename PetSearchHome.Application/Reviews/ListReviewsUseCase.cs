using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Application.Reviews
{
    public sealed record ListReviewsRequest(Guid ListingId);

    public class ListReviewsUseCase : IUseCase<ListReviewsRequest, IReadOnlyList<Review>>
    {
        private readonly IReviewRepository _reviews;

        public ListReviewsUseCase(IReviewRepository reviews)
        {
            _reviews = reviews;
        }

        public Task<IReadOnlyList<Review>> ExecuteAsync(ListReviewsRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            return _reviews.ListByListingAsync(request.ListingId, cancellationToken);
        }
    }
}
