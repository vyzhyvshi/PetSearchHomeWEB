using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;

namespace PetSearchHome_WEB.Infrastructure.Repositories
{
    public class InMemoryListingRepository : IListingRepository
    {
        private readonly List<PetListing> _listings;

        public InMemoryListingRepository()
        {
            _listings = Seed();
        }

        public Task<IReadOnlyList<PetListing>> GetFeaturedAsync(int take, CancellationToken cancellationToken = default)
        {
            var featured = _listings
                .OrderByDescending(x => x.IsUrgent)
                .ThenByDescending(x => x.ListedAt)
                .Take(take)
                .ToList()
                .AsReadOnly();

            return Task.FromResult<IReadOnlyList<PetListing>>(featured);
        }

        public Task AddAsync(PetListing listing, CancellationToken cancellationToken = default)
        {
            _listings.Add(new PetListing
            {
                Id = Guid.NewGuid(),
                Title = listing.Title,
                AnimalType = listing.AnimalType,
                Location = listing.Location,
                ListedAt = DateTimeOffset.UtcNow,
                Description = listing.Description,
                IsUrgent = listing.IsUrgent
            });

            return Task.CompletedTask;
        }

        private static List<PetListing> Seed()
        {
            return new List<PetListing>
            {
                new()
                {
                    Title = "Labrador Rocky",
                    AnimalType = "Dog",
                    Location = "Kyiv",
                    ListedAt = DateTimeOffset.UtcNow.AddHours(-6),
                    Description = "Friendly, 2 years old, loves long walks.",
                    IsUrgent = true
                },
                new()
                {
                    Title = "Cat Mira",
                    AnimalType = "Cat",
                    Location = "Lviv",
                    ListedAt = DateTimeOffset.UtcNow.AddDays(-1),
                    Description = "Very gentle, sterilized, indoor only.",
                    IsUrgent = false
                },
                new()
                {
                    Title = "Puppy Max",
                    AnimalType = "Dog",
                    Location = "Odesa",
                    ListedAt = DateTimeOffset.UtcNow.AddHours(-12),
                    Description = "3 months, leash trained.",
                    IsUrgent = true
                },
                new()
                {
                    Title = "Cat Bonnie",
                    AnimalType = "Cat",
                    Location = "Dnipro",
                    ListedAt = DateTimeOffset.UtcNow.AddDays(-2),
                    Description = "Calm temperament, good with kids.",
                    IsUrgent = false
                },
                new()
                {
                    Title = "Dog Rudy",
                    AnimalType = "Dog",
                    Location = "Kharkiv",
                    ListedAt = DateTimeOffset.UtcNow.AddHours(-30),
                    Description = "Vaccinated, playful, medium size.",
                    IsUrgent = false
                },
                new()
                {
                    Title = "Cat Lola",
                    AnimalType = "Cat",
                    Location = "Vinnytsia",
                    ListedAt = DateTimeOffset.UtcNow.AddHours(-3),
                    Description = "Loves attention, sterilized.",
                    IsUrgent = true
                }
            };
        }
    }
}
