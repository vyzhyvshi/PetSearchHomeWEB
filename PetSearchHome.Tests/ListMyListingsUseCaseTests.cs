using Moq;
using PetSearchHome_WEB.Application.Listing;
using PetSearchHome_WEB.Application.Shared;
using PetSearchHome_WEB.Domain.Entities;
using PetSearchHome_WEB.Domain.Interfaces;
using PetSearchHome_WEB.Domain.ValueObjects;
using Xunit;

namespace PetSearchHome.Tests
{
    public class ListMyListingsUseCaseTests
    {
        private readonly Mock<IListingRepository> _listingsMock;
        private readonly ListMyListingsUseCase _useCase;

        public ListMyListingsUseCaseTests()
        {
            _listingsMock = new Mock<IListingRepository>();
            _useCase = new ListMyListingsUseCase(_listingsMock.Object);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserNotAuthorized_ReturnsFailure()
        {
            var request = new ListMyListingsRequest();
            var authContext = new AuthContext { UserId = null, Role = Role.Guest };

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.False(result.IsSuccess);
            Assert.Contains("авторизація", result.ErrorMessage ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task ExecuteAsync_WhenUserAuthorized_ReturnsUserListings()
        {
            var userId = Guid.NewGuid();
            var expectedListings = new List<PetListing>
            {
                new PetListing { Id = Guid.NewGuid(), Title = "Мій песик", OwnerId = userId },
                new PetListing { Id = Guid.NewGuid(), Title = "Моє кошеня", OwnerId = userId }
            };

            _listingsMock.Setup(repo => repo.ListByOwnerAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedListings);

            var request = new ListMyListingsRequest();
            var authContext = new AuthContext { UserId = userId, Role = Role.Person };

            var result = await _useCase.ExecuteAsync(request, authContext);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count);
        }
    }
}