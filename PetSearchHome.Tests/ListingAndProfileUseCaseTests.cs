using Moq;
using PetSearchHome_WEB.Application.Listing;
using PetSearchHome_WEB.Application.Profiles;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using Xunit;

namespace PetSearchHome.Tests
{
    public class ListingAndProfileUseCaseTests
    {
        [Fact]
        public async Task CreateListing_WhenPersonCreates_AddsListingAndQueuesModeration()
        {
            var listings = new Mock<IListingRepository>();
            var moderation = new Mock<IModerationQueue>();
            var useCase = new CreateListingUseCase(listings.Object, moderation.Object);
            var auth = new AuthContext { UserId = Guid.NewGuid(), Role = Role.Person };
            var request = new CreateListingRequest(
                "Знайдено собаку",
                "Собака",
                "Київ",
                "Дружня, з нашийником",
                true,
                new[] { "https://example.com/1.jpg" });

            var result = await useCase.ExecuteAsync(request, auth, CancellationToken.None);

            Assert.True(result.IsSuccess);
            listings.Verify(repo => repo.AddAsync(
                It.Is<PetListing>(listing =>
                    listing.OwnerId == auth.UserId!.Value &&
                    listing.Status == ListingStatus.PendingModeration &&
                    listing.PhotoUrls.Count == 1),
                It.IsAny<CancellationToken>()), Times.Once);
            moderation.Verify(queue => queue.EnqueueAsync(
                It.Is<PetListing>(listing => listing.Id == result.Value),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task EditListing_WhenOwnerEdits_ResetsStatusToPendingModeration()
        {
            var listingId = Guid.NewGuid();
            var ownerId = Guid.NewGuid();
            var stored = new PetListing
            {
                Id = listingId,
                OwnerId = ownerId,
                OwnerRole = Role.Person,
                Title = "Old",
                AnimalType = "Dog",
                Location = "Kyiv",
                Status = ListingStatus.Published
            };

            var listings = new Mock<IListingRepository>();
            listings.Setup(repo => repo.GetByIdAsync(listingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(stored);

            var useCase = new EditListingUseCase(listings.Object);
            var auth = new AuthContext { UserId = ownerId, Role = Role.Person };
            var request = new EditListingRequest(
                listingId,
                "New",
                "Dog",
                "Lviv",
                "Updated",
                false,
                new[] { "https://example.com/2.jpg" });

            var result = await useCase.ExecuteAsync(request, auth, CancellationToken.None);

            Assert.True(result.IsSuccess);
            listings.Verify(repo => repo.UpdateAsync(
                It.Is<PetListing>(listing =>
                    listing.Title == "New" &&
                    listing.Location == "Lviv" &&
                    listing.Status == ListingStatus.PendingModeration &&
                    listing.PhotoUrls.Count == 1),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ViewProfileDetails_ForGuest_ShowsOnlyPublishedListingsAndAverageRating()
        {
            var userId = Guid.NewGuid();
            var publishedId = Guid.NewGuid();
            var pendingId = Guid.NewGuid();

            var users = new Mock<IUserRepository>();
            users.Setup(repo => repo.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User
                {
                    Id = userId,
                    Email = "shelter@test.com",
                    DisplayName = "Best Shelter",
                    Role = Role.Shelter
                });

            var listings = new Mock<IListingRepository>();
            listings.Setup(repo => repo.ListByOwnerAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PetListing>
                {
                    new() { Id = publishedId, OwnerId = userId, Title = "A", Status = ListingStatus.Published },
                    new() { Id = pendingId, OwnerId = userId, Title = "B", Status = ListingStatus.PendingModeration }
                });

            var shelters = new Mock<IShelterRepository>();
            shelters.Setup(repo => repo.GetProfileAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ShelterProfile
                {
                    ShelterId = userId,
                    DisplayName = "Best Shelter",
                    Description = "Safe place"
                });

            var reviews = new Mock<IReviewRepository>();
            reviews.Setup(repo => repo.ListByListingAsync(publishedId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Review>
                {
                    new() { ListingId = publishedId, Rating = 4 },
                    new() { ListingId = publishedId, Rating = 5 }
                });
            reviews.Setup(repo => repo.ListByListingAsync(pendingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Review>());

            var useCase = new ViewProfileDetailsUseCase(users.Object, listings.Object, shelters.Object, reviews.Object);
            var auth = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await useCase.ExecuteAsync(new ViewProfileDetailsRequest(userId), auth, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Single(result!.Listings);
            Assert.Equal(ListingStatus.Published, result.Listings[0].Status);
            Assert.Equal(4.5, result.Rating);
            Assert.Equal(2, result.ReviewsCount);
        }
    }
}
