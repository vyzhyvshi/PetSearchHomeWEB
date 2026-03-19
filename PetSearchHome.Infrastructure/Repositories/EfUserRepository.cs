using Microsoft.EntityFrameworkCore;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using PetSearchHome_WEB.Infrastructure.Persistence;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Repositories
{
    /// <summary>
    /// EF-backed user repository that maps domain users to persisted UserEntity rows.
    /// Domain Id (Guid) is stored in the database as DomainId alongside the int identity PK.
    /// </summary>
    public class EfUserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;

        public EfUserRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var userId = FromDomainId(id);
            var entity = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            return entity is null ? null : Map(entity);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            return entity is null ? null : Map(entity);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            var entity = new UserEntity
            {
                Email = user.Email,
                PasswordHash = user.PasswordHash,
                Role = user.Role,
                IsActive = !user.IsBlocked,
                CreatedAt = DateTime.UtcNow,
                IsVerified = false
            };

            await _db.Users.AddAsync(entity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateRoleAsync(Guid id, Role role, CancellationToken cancellationToken = default)
        {
            var userId = FromDomainId(id);
            var entity = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            if (entity is null) return;
            entity.Role = role;
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdatePasswordAsync(Guid id, string passwordHash, CancellationToken cancellationToken = default)
        {
            var userId = FromDomainId(id);
            var entity = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            if (entity is null) return;
            entity.PasswordHash = passwordHash;
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateProfileAsync(User user, CancellationToken cancellationToken = default)
        {
            var userId = FromDomainId(user.Id);
            var entity = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            if (entity is null) return;

            entity.Email = user.Email;
            entity.Role = user.Role;
            entity.IsActive = !user.IsBlocked;

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task SetBlockedAsync(Guid id, bool isBlocked, CancellationToken cancellationToken = default)
        {
            var userId = FromDomainId(id);
            var entity = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            if (entity is null) return;
            entity.IsActive = !isBlocked;
            await _db.SaveChangesAsync(cancellationToken);
        }

        private static Guid ToDomainId(int userId)
        {
            Span<byte> bytes = stackalloc byte[16];
            bytes.Clear();
            BitConverter.TryWriteBytes(bytes, userId);
            return new Guid(bytes);
        }

        private static int FromDomainId(Guid id)
        {
            var bytes = id.ToByteArray();
            return BitConverter.ToInt32(bytes, 0);
        }

        private static User Map(UserEntity entity) =>
            new User
            {
                Id = ToDomainId(entity.UserId),
                Email = entity.Email,
                DisplayName = entity.Email, // fallback, оскільки в БД немає колонки display_name
                PasswordHash = entity.PasswordHash,
                Role = entity.Role,
                IsBlocked = !entity.IsActive
            };
    }
}
