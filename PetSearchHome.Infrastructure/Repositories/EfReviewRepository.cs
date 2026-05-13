using Microsoft.EntityFrameworkCore;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Infrastructure.Persistence;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Repositories;

public class EfReviewRepository : IReviewRepository
{
    private readonly ApplicationDbContext _db;

    public EfReviewRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Review review, CancellationToken cancellationToken = default)
    {
        var entity = new ReviewEntity
        {
            ReviewerId = review.AuthorId,
            ReviewedId = review.ReviewedUserId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt.UtcDateTime
        };

        await _db.Reviews.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Review>> ListByReviewedUserAsync(int reviewedUserId, CancellationToken cancellationToken = default)
    {
        var reviewedId = reviewedUserId;

        var reviews = await _db.Reviews
            .AsNoTracking()
            .Where(r => r.ReviewedId == reviewedId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new Review
            {
                Id = r.ReviewId,
                AuthorId = r.ReviewerId,
                ReviewedUserId = r.ReviewedId,
                Rating = (byte)r.Rating,
                Comment = r.Comment,
                CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(r.CreatedAt, DateTimeKind.Utc))
            })
            .ToListAsync(cancellationToken);

        return reviews;
    }

}
