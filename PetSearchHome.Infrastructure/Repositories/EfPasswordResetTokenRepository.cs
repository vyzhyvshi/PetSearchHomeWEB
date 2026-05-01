using Microsoft.EntityFrameworkCore;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Infrastructure.Persistence;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Repositories;

public class EfPasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly ApplicationDbContext _db;

    public EfPasswordResetTokenRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
    {
        await _db.PasswordResetTokens.AddAsync(new PasswordResetTokenEntity
        {
            UserId = FromDomainId(token.UserId),
            TokenHash = token.TokenHash,
            ExpiresAt = token.ExpiresAt.UtcDateTime,
            UsedAt = token.UsedAt?.UtcDateTime,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PasswordResetToken?> GetUsableAsync(Guid userId, string tokenHash, CancellationToken cancellationToken = default)
    {
        var userKey = FromDomainId(userId);
        var now = DateTime.UtcNow;
        var entity = await _db.PasswordResetTokens
            .AsNoTracking()
            .Where(t => t.UserId == userKey && t.TokenHash == tokenHash && t.UsedAt == null && t.ExpiresAt > now)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null
            ? null
            : new PasswordResetToken
            {
                Id = ToDomainId(entity.PasswordResetTokenId),
                UserId = ToDomainId(entity.UserId),
                TokenHash = entity.TokenHash,
                ExpiresAt = new DateTimeOffset(DateTime.SpecifyKind(entity.ExpiresAt, DateTimeKind.Utc)),
                UsedAt = entity.UsedAt.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(entity.UsedAt.Value, DateTimeKind.Utc))
                    : null
            };
    }

    public async Task MarkUsedAsync(Guid tokenId, CancellationToken cancellationToken = default)
    {
        var id = FromDomainId(tokenId);
        var entity = await _db.PasswordResetTokens.FirstOrDefaultAsync(t => t.PasswordResetTokenId == id, cancellationToken);
        if (entity is null)
        {
            return;
        }

        entity.UsedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static Guid ToDomainId(int value)
    {
        Span<byte> bytes = stackalloc byte[16];
        bytes.Clear();
        BitConverter.TryWriteBytes(bytes, value);
        return new Guid(bytes);
    }

    private static int FromDomainId(Guid id)
    {
        var bytes = id.ToByteArray();
        return BitConverter.ToInt32(bytes, 0);
    }
}
