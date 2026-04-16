using Microsoft.EntityFrameworkCore;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using PetSearchHome_WEB.Infrastructure.Persistence;
using PetSearchHome_WEB.Infrastructure.Persistence.Entities;

namespace PetSearchHome_WEB.Infrastructure.Repositories
{
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
            var entity = await _db.Users
                .AsNoTracking()
                .Include(u => u.IndividualProfile)
                .Include(u => u.ShelterProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

            return entity is null ? null : Map(entity);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var entity = await _db.Users
                .AsNoTracking()
                .Include(u => u.IndividualProfile)
                .Include(u => u.ShelterProfile)
                .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

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

            if (user.Role == Role.Shelter)
            {
                entity.ShelterProfile = new ShelterProfileEntity
                {
                    Name = user.DisplayName,
                    ContactPerson = user.DisplayName,
                    Address = string.Empty,
                    Phone = string.Empty
                };
            }
            else
            {
                entity.IndividualProfile = new IndividualProfileEntity
                {
                    FirstName = user.DisplayName,
                    LastName = string.Empty,
                    Phone = string.Empty,
                    City = string.Empty,
                    District = string.Empty
                };
            }

            await _db.Users.AddAsync(entity, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateRoleAsync(Guid id, Role role, CancellationToken cancellationToken = default)
        {
            var userId = FromDomainId(id);
            var entity = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            if (entity is null)
            {
                return;
            }

            entity.Role = role;
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdatePasswordAsync(Guid id, string passwordHash, CancellationToken cancellationToken = default)
        {
            var userId = FromDomainId(id);
            var entity = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            if (entity is null)
            {
                return;
            }

            entity.PasswordHash = passwordHash;
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateProfileAsync(User user, CancellationToken cancellationToken = default)
        {
            var userId = FromDomainId(user.Id);
            var entity = await _db.Users
                .Include(u => u.IndividualProfile)
                .Include(u => u.ShelterProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);

            if (entity is null)
            {
                return;
            }

            entity.Email = user.Email;
            entity.Role = user.Role;
            entity.IsActive = !user.IsBlocked;

            if (user.Role == Role.Shelter)
            {
                entity.ShelterProfile ??= new ShelterProfileEntity
                {
                    UserId = entity.UserId,
                    Address = string.Empty,
                    Phone = string.Empty
                };
                entity.ShelterProfile.Name = user.DisplayName;
                entity.ShelterProfile.ContactPerson = user.DisplayName;
            }
            else
            {
                entity.IndividualProfile ??= new IndividualProfileEntity
                {
                    UserId = entity.UserId,
                    LastName = string.Empty,
                    Phone = string.Empty,
                    City = string.Empty,
                    District = string.Empty
                };
                entity.IndividualProfile.FirstName = user.DisplayName;
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task SetBlockedAsync(Guid id, bool isBlocked, CancellationToken cancellationToken = default)
        {
            var userId = FromDomainId(id);
            var entity = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
            if (entity is null)
            {
                return;
            }

            entity.IsActive = !isBlocked;
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<User>> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<User>();
            }

            var pattern = $"%{query.Trim()}%";
            var entities = await _db.Users
                .AsNoTracking()
                .Include(u => u.IndividualProfile)
                .Include(u => u.ShelterProfile)
                .Where(u => EF.Functions.Like(u.Email, pattern)
                    || (u.ShelterProfile != null && EF.Functions.Like(u.ShelterProfile.Name, pattern))
                    || (u.IndividualProfile != null && EF.Functions.Like(u.IndividualProfile.FirstName, pattern))
                    || (u.IndividualProfile != null && EF.Functions.Like(u.IndividualProfile.LastName, pattern)))
                .OrderBy(u => u.Email)
                .ToListAsync(cancellationToken);

            return entities.Select(Map).ToList();
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
            new()
            {
                Id = ToDomainId(entity.UserId),
                Email = entity.Email,
                DisplayName = ResolveDisplayName(entity),
                PasswordHash = entity.PasswordHash,
                Role = entity.Role,
                IsBlocked = !entity.IsActive
            };

        private static string ResolveDisplayName(UserEntity entity)
        {
            if (entity.Role == Role.Shelter)
            {
                return string.IsNullOrWhiteSpace(entity.ShelterProfile?.Name)
                    ? entity.Email
                    : entity.ShelterProfile.Name;
            }

            return string.IsNullOrWhiteSpace(entity.IndividualProfile?.FirstName)
                ? entity.Email
                : entity.IndividualProfile.FirstName;
        }
    }
}
