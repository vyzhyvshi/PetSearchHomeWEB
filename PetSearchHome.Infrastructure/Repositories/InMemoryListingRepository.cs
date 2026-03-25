using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;

namespace PetSearchHome_WEB.Infrastructure.Repositories
{

    public class InMemoryListingRepository : IListingRepository, ISearchGateway
    {
        private readonly List<PetListing> _listings;

        public InMemoryListingRepository()
        {
            _listings = Seed();
        }

        public Task<IReadOnlyList<PetListing>> GetFeaturedAsync(int take, CancellationToken cancellationToken = default)
        {
            var featured = _listings
                .Where(x => x.Status == Domain.ValueObjects.ListingStatus.Published)
                .OrderByDescending(x => x.IsUrgent)
                .ThenByDescending(x => x.ListedAt)
                .Take(take)
                .ToList()
                .AsReadOnly();

            return Task.FromResult<IReadOnlyList<PetListing>>(featured);
        }

        public Task<PetListing?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var listing = _listings.SingleOrDefault(x => x.Id == id);
            return Task.FromResult(listing);
        }

        public Task<IReadOnlyList<PetListing>> ListByOwnerAsync(Guid ownerId, CancellationToken cancellationToken = default)
        {
            var results = _listings
                .Where(x => x.OwnerId == ownerId)
                .OrderByDescending(x => x.ListedAt)
                .ToList()
                .AsReadOnly();

            return Task.FromResult<IReadOnlyList<PetListing>>(results);
        }
        public Task<IReadOnlyList<PetListing>> ListByStatusAsync(Domain.ValueObjects.ListingStatus status, CancellationToken cancellationToken = default)
        {
            var results = _listings
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.ListedAt)
                .ToList()
                .AsReadOnly();

            return Task.FromResult<IReadOnlyList<PetListing>>(results);
        }

        public Task AddAsync(PetListing listing, CancellationToken cancellationToken = default)
        {
            _listings.Add(new PetListing
            {
                Id = Guid.NewGuid(),
                OwnerId = listing.OwnerId == Guid.Empty ? Guid.NewGuid() : listing.OwnerId,
                OwnerRole = listing.OwnerRole,
                Title = listing.Title,
                AnimalType = listing.AnimalType,
                Location = listing.Location,
                Status = listing.Status,
                ListedAt = DateTimeOffset.UtcNow,
                Description = listing.Description,
                IsUrgent = listing.IsUrgent
            });

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<PetListing>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
        {
            var results = _listings
                .Where(x => ids.Contains(x.Id))
                .ToList()
                .AsReadOnly();

            return Task.FromResult<IReadOnlyList<PetListing>>(results);
        }

        public Task UpdateAsync(PetListing listing, CancellationToken cancellationToken = default)
        {
            var index = _listings.FindIndex(x => x.Id == listing.Id);
            if (index >= 0)
            {
                _listings[index] = listing;
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _listings.RemoveAll(x => x.Id == id);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<PetListing>> SearchAsync(SearchFilters filters, CancellationToken cancellationToken = default)
        {
            IEnumerable<PetListing> query = _listings
                .Where(l => l.Status == Domain.ValueObjects.ListingStatus.Published);

            if (!string.IsNullOrWhiteSpace(filters.SearchQuery))
            {
                query = query.Where(l =>
                    l.Title.Contains(filters.SearchQuery, StringComparison.OrdinalIgnoreCase)
                    || (l.Description?.Contains(filters.SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            if (!string.IsNullOrWhiteSpace(filters.AnimalType))
            {
                query = query.Where(l => string.Equals(l.AnimalType, filters.AnimalType, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filters.Location))
            {
                query = query.Where(l => l.Location.Contains(filters.Location, StringComparison.OrdinalIgnoreCase));
            }

            if (filters.IsUrgent.HasValue)
            {
                query = query.Where(l => l.IsUrgent == filters.IsUrgent.Value);
            }

            var results = query
                .OrderByDescending(l => l.IsUrgent)
                .ThenByDescending(l => l.ListedAt)
                .ToList()
                .AsReadOnly();

            return Task.FromResult<IReadOnlyList<PetListing>>(results);
        }

        private static List<PetListing> Seed()
        {
            return new List<PetListing>
            {
                new()
                {
                    OwnerId = Guid.NewGuid(),
                    OwnerRole = Domain.ValueObjects.Role.Person,
                    Title = "Labrador Rocky",
                    AnimalType = "Dog",
                    Location = "Kyiv",
                    Status = Domain.ValueObjects.ListingStatus.Published,
                    ListedAt = DateTimeOffset.UtcNow.AddHours(-6),
                    Description = "Friendly, 2 years old, loves long walks.",
                    IsUrgent = true
                },
                new()
                {
                    OwnerId = Guid.NewGuid(),
                    OwnerRole = Domain.ValueObjects.Role.Person,
                    Title = "Cat Mira",
                    AnimalType = "Cat",
                    Location = "Lviv",
                    Status = Domain.ValueObjects.ListingStatus.Published,
                    ListedAt = DateTimeOffset.UtcNow.AddDays(-1),
                    Description = "Very gentle, sterilized, indoor only.",
                    IsUrgent = false
                },
                new()
                {
                    OwnerId = Guid.NewGuid(),
                    OwnerRole = Domain.ValueObjects.Role.Shelter,
                    Title = "Puppy Max",
                    AnimalType = "Dog",
                    Location = "Odesa",
                    Status = Domain.ValueObjects.ListingStatus.PendingModeration,
                    ListedAt = DateTimeOffset.UtcNow.AddHours(-12),
                    Description = "3 months, leash trained.",
                    IsUrgent = true
                },
                new()
                {
                    OwnerId = Guid.NewGuid(),
                    OwnerRole = Domain.ValueObjects.Role.Person,
                    Title = "Cat Bonnie",
                    AnimalType = "Cat",
                    Location = "Dnipro",
                    Status = Domain.ValueObjects.ListingStatus.Published,
                    ListedAt = DateTimeOffset.UtcNow.AddDays(-2),
                    Description = "Calm temperament, good with kids.",
                    IsUrgent = false
                },
                new()
                {
                    OwnerId = Guid.NewGuid(),
                    OwnerRole = Domain.ValueObjects.Role.Person,
                    Title = "Dog Rudy",
                    AnimalType = "Dog",
                    Location = "Kharkiv",
                    Status = Domain.ValueObjects.ListingStatus.Rejected,
                    ListedAt = DateTimeOffset.UtcNow.AddHours(-30),
                    Description = "Vaccinated, playful, medium size.",
                    IsUrgent = false
                },
                new()
                {
                    OwnerId = Guid.NewGuid(),
                    OwnerRole = Domain.ValueObjects.Role.Shelter,
                    Title = "Cat Lola",
                    AnimalType = "Cat",
                    Location = "Vinnytsia",
                    Status = Domain.ValueObjects.ListingStatus.Published,
                    ListedAt = DateTimeOffset.UtcNow.AddHours(-3),
                    Description = "Loves attention, sterilized.",
                    IsUrgent = true
                }
            };
        }
    }
}
