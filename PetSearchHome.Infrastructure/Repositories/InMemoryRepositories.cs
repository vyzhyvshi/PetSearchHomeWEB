using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Infrastructure.Repositories
{
    public class InMemorySearchGateway : ISearchGateway
    {
        private readonly List<PetListing> _listings;

        public InMemorySearchGateway()
        {
            _listings = new List<PetListing>
            {
                new PetListing
                {
                    Id = Guid.Parse("dd284e4c-4f2f-4a0c-8986-1a78a2bf0000"),
                    Title = "Rocky (Лабрадор)",
                    AnimalType = "Dog",
                    Location = "Kyiv"
                },
                new PetListing
                {
                    Id = Guid.Parse("a3e42b80-7aba-44a4-9d1f-ede2a2900000"),
                    Title = "Mira (Британська короткошерста)",
                    AnimalType = "Cat",
                    Location = "Lviv"
                },
                new PetListing
                {
                    Id = Guid.Parse("0fdff0e9-5d8a-45fa-87bc-ded9c7d34300"),
                    Title = "Nora",
                    AnimalType = "Dog",
                    Location = "Odesa"
                }
            };
        }

        public Task<IReadOnlyList<PetListing>> SearchAsync(SearchFilters filters, CancellationToken cancellationToken = default)
        {
            // Повертаємо наш список. Поки що без фільтрації, просто віддаємо всіх.
            return Task.FromResult<IReadOnlyList<PetListing>>(_listings);
        }
    }

    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();

        public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.FirstOrDefault(u => u.Email == email));

        public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

        public Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            var newUser = user.Id == Guid.Empty
                ? new User
                {
                    Id = Guid.NewGuid(),
                    Email = user.Email,
                    PasswordHash = user.PasswordHash,
                    Role = user.Role,
                    DisplayName = user.DisplayName,
                    IsBlocked = user.IsBlocked
                }
                : user;

            _users.Add(newUser);
            return Task.CompletedTask;
        }

        public Task SetBlockedAsync(Guid id, bool isBlocked, CancellationToken cancellationToken = default)
        {
            var index = _users.FindIndex(u => u.Id == id);
            if (index >= 0)
            {
                var oldUser = _users[index];
                _users[index] = new User
                {
                    Id = oldUser.Id,
                    Email = oldUser.Email,
                    PasswordHash = oldUser.PasswordHash,
                    Role = oldUser.Role,
                    DisplayName = oldUser.DisplayName,
                    IsBlocked = isBlocked // Ось тут ми оновлюємо статус
                };
            }
            return Task.CompletedTask;
        }

        public Task UpdateProfileAsync(User user, CancellationToken cancellationToken = default)
        {
            var index = _users.FindIndex(u => u.Id == user.Id);
            if (index >= 0) _users[index] = user;
            return Task.CompletedTask;
        }

        public Task UpdateRoleAsync(Guid id, Role role, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdatePasswordAsync(Guid id, string passwordHash, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public class InMemoryShelterRepository : IShelterRepository
    {
        private readonly List<ShelterProfile> _profiles = new();

        public Task<ShelterProfile?> GetProfileAsync(Guid shelterId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpsertProfileAsync(ShelterProfile profile, CancellationToken cancellationToken = default)
        {
            var existing = _profiles.FirstOrDefault(p => p.ShelterId == profile.ShelterId);
            if (existing != null) _profiles.Remove(existing);
            _profiles.Add(profile);
            return Task.CompletedTask;
        }
    }

    public class InMemoryComplaintRepository : IComplaintRepository
    {
        private readonly List<Complaint> _complaints = new();
        public Task UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<Complaint>> ListOpenAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Complaint>>(_complaints);

        public Task AddAsync(Complaint complaint, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public class InMemoryFavoriteRepository : IFavoriteRepository
    {
        private readonly List<Favorite> _favorites = new();

        public Task<Favorite?> GetAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default)
            => Task.FromResult(_favorites.FirstOrDefault(f => f.UserId == userId && f.ListingId == listingId));

        public Task RemoveAsync(Guid userId, Guid listingId, CancellationToken cancellationToken = default)
        {
            _favorites.RemoveAll(f => f.UserId == userId && f.ListingId == listingId);
            return Task.CompletedTask;
        }

        public Task AddAsync(Favorite favorite, CancellationToken cancellationToken = default)
        {
            _favorites.Add(favorite);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Guid>> ListIdsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Guid>>(_favorites.Where(f => f.UserId == userId).Select(f => f.ListingId).ToList());

        public Task<IReadOnlyList<Favorite>> ListByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public class InMemoryReviewRepository : IReviewRepository
    {
        public Task AddAsync(Review review, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<Review>> ListByListingAsync(Guid listingId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public class InMemoryOrgStatsRepository : IOrgStatsRepository
    {
        public Task<OrgStats?> GetAsync(Guid shelterId, CancellationToken cancellationToken = default) => Task.FromResult<OrgStats?>(null);

        public Task UpsertAsync(OrgStats stats, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    public class InMemoryNotificationGateway : INotificationGateway
    {
        public Task NotifyAsync(Guid recipientId, string message, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    public class SimplePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => password; // Лише для In-Memory тестування!
        public bool Verify(string password, string hashed) => password == hashed;
    }

    public class DummyAuthTokenService : IAuthTokenService
    {
        public string IssueToken(User user) => "dummy-jwt-token";
    }

    public class InMemoryModerationQueue : IModerationQueue
    {
        private readonly List<Guid> _queue = new();

        public Task EnqueueAsync(Guid listingId, CancellationToken cancellationToken = default)
        {
            if (!_queue.Contains(listingId))
            {
                _queue.Add(listingId);
            }
            return Task.CompletedTask;
        }

        public Task EnqueueAsync(PetListing listing, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveFromQueueAsync(Guid listingId, CancellationToken cancellationToken = default)
        {
            _queue.Remove(listingId);
            return Task.CompletedTask;
        }
    }
}