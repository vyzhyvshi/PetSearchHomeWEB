using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.Policies;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Application.Reviews
{
    public sealed record LeaveReviewRequest(Guid ReviewedUserId, byte Rating, string Comment, bool HasInteraction);

    public class LeaveReviewUseCase : IUseCase<LeaveReviewRequest, Guid>
    {
        private readonly IReviewRepository _reviews;

        public LeaveReviewUseCase(IReviewRepository reviews)
        {
            _reviews = reviews;
        }

        public async Task<Guid> ExecuteAsync(LeaveReviewRequest request, AuthContext authContext, CancellationToken cancellationToken = default)
        {
            if (authContext.UserId is null || !ReviewPolicy.CanLeaveReview(authContext.Role, request.HasInteraction))
            {
                throw new UnauthorizedAccessException("Cannot leave review.");
            }

            if (authContext.UserId == request.ReviewedUserId)
            {
                throw new InvalidOperationException("Cannot leave a review for your own profile.");
            }

            if (request.Rating is < 1 or > 5)
            {
                throw new InvalidOperationException("Rating must be between 1 and 5.");
            }

            var review = new Review
            {
                ReviewedUserId = request.ReviewedUserId,
                AuthorId = authContext.UserId.Value,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _reviews.AddAsync(review, cancellationToken);
            return review.Id;
        }
    }
}
